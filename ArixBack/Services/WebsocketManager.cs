using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace ArixBack.Services
{
    public class WebsocketManager
    {
        private readonly ConcurrentDictionary<string, WebSocket> _connections = new();

        public void AddConnection(string id, WebSocket webSocket) => _connections.TryAdd(id, webSocket);

        public void RemoveConnection(string id) => _connections.TryRemove(id, out _);

        public IEnumerable<WebSocket> GetAllConnections() => _connections.Values;

        public WebSocket? GetConnection(string id)
        {
            _connections.TryGetValue(id, out var webSocket);
            return webSocket;
        }

        public async Task SendToPlayer(string playerId, object message)
        {
            if (!_connections.TryGetValue(playerId, out var ws)) return;
            if (ws.State != WebSocketState.Open) return;
            var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            await ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
