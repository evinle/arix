using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.AspNetCore.Authorization;
using ArixBack.Services;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace ArixBack.Controllers
{
    [Authorize]
    [ApiController]
    [Route("Websocket")]
    public class Matchmaking : ControllerBase
    {
        private readonly ILogger<Matchmaking> _logger;
        private readonly MatchmakingService _matchmakingService;
        private readonly MatchSessionManager _matchSessionManager;
        private readonly PlayerService _playerService;

        public Matchmaking(
            ILogger<Matchmaking> logger,
            MatchmakingService matchmakingService,
            MatchSessionManager matchSessionManager,
            PlayerService playerService)
        {
            _logger = logger;
            _matchmakingService = matchmakingService;
            _matchSessionManager = matchSessionManager;
            _playerService = playerService;
        }

        [HttpGet("ws")]
        public async Task Matchmake()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var userName = User.FindFirstValue(JwtRegisteredClaimNames.NameId);

                if (userName != null)
                {
                    var player = await _playerService.GetPlayerFromUsername(userName);
                    if (player == null)
                    {
                        HttpContext.Response.StatusCode = 404;
                        return;
                    }

                    using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                    _logger.LogInformation($"WebSocket connection established for player {player.Username}");

                    await HandleWebSocketAsync(webSocket, player);
                }
                else
                {
                    HttpContext.Response.StatusCode = 401;
                }
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }
        }

        private async Task HandleWebSocketAsync(WebSocket webSocket, Models.Player player)
        {
            var buffer = new byte[1024 * 4];

            try
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                while (!result.CloseStatus.HasValue)
                {
                    var messageJson = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    try
                    {
                        _logger.LogInformation($"Raw message received from {player.Username}: {messageJson}");
                        var doc = JsonDocument.Parse(messageJson);
                        var type = doc.RootElement.GetProperty("type").GetString();
                        var payload = doc.RootElement.GetProperty("payload");

                        await ProcessMessageAsync(player, type, payload, webSocket);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error processing message from {player.Username}");
                    }

                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }

                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"WebSocket error for {player.Username}");
            }
            finally
            {
                _matchmakingService.ClearPlayer(player.Id);
                await _matchSessionManager.HandleDisconnect(player.Id);
                _logger.LogInformation($"WebSocket connection closed for player {player.Username}");
            }
        }

        private async Task ProcessMessageAsync(Models.Player player, string type, JsonElement payload, WebSocket socket)
        {
            switch (type)
            {
                case "JOIN_QUEUE":
                    _logger.LogInformation($"{player.Username} joining queue");
                    _matchmakingService.Enqueue(player, socket);
                    var queueCount = _matchmakingService.GetUniquePlayerCount();
                    var confirmation = JsonSerializer.Serialize(new { 
                        type = "JOINED_QUEUE", 
                        payload = new { 
                            message = "You're in! Waiting for a match...",
                            playersInQueue = queueCount
                        } 
                    });
                    var bytes = Encoding.UTF8.GetBytes(confirmation);
                    await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                    break;

                case "SUBMIT_ANSWER":
                    var session = _matchSessionManager.GetSessionByPlayerId(player.Id);
                    if (session != null)
                    {
                        await session.HandleMessageAsync(player.Id, type, payload);
                    }
                    break;

                default:
                    _logger.LogWarning($"Unknown message type: {type} from {player.Username}");
                    break;
            }
        }
    }
}
