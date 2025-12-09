using System;
using System.Net.WebSockets;
using System.Text;
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
    // [Authorize]
    [ApiController]
    [Route("Websocket")]
    public class Matchmaking : ControllerBase
    {
        private readonly ILogger<Matchmaking> _logger;
        private WebsocketManager _websocketManager;
        private PlayerService _playerService;

        public Matchmaking(ILogger<Matchmaking> logger, WebsocketManager websocketManager, PlayerService playerService)
        {
            _logger = logger;
            _websocketManager = websocketManager;
            _playerService = playerService;
        }

        [HttpGet("ws")]
        public async Task Matchmake()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                // var userName = User.FindFirstValue(JwtRegisteredClaimNames.NameId);
                var userName = "lol";

                if (userName != null)
                {
                    using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

                    _logger.Log(LogLevel.Information, "WebSocket connection established");

                    var bsonId = (await _playerService.GetPlayerFromUsername(userName))?.Id;
                    string id = bsonId.ToString();
                    _websocketManager.AddConnection(id, webSocket);

                    await Echo(webSocket, id);

                }
                else
                {
                    HttpContext.Response.StatusCode = 424;
                }
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }
        }

        [HttpGet("GetAllConnections")]
        public IEnumerable<WebSocket> GetAllConnections()
        {
            return _websocketManager.GetAllConnections();
        }

        private async Task Echo(WebSocket webSocket, string id)
        {

            //remove later - just test to see if websocket works
            if (webSocket.State == WebSocketState.Open)
            {
                var buffer = new byte[1024 * 4];
                _logger.Log(LogLevel.Information, "Buffer opened");
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                _logger.Log(LogLevel.Information, "Message received from Client");

                while (!result.CloseStatus.HasValue)
                {
                    var serverMsg = Encoding.UTF8.GetBytes($"Server: Hello. You said: {Encoding.UTF8.GetString(buffer)}");
                    await webSocket.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
                    _logger.Log(LogLevel.Information, "Message sent to Client");

                    buffer = new byte[1024 * 4];
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    _logger.Log(LogLevel.Information, "Message received from Client");

                }
                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                _logger.Log(LogLevel.Information, "WebSocket connection closed");
                _websocketManager.RemoveConnection(id);
            }
        }
    }
}