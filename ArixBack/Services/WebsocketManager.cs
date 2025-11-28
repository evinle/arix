using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace ArixBack.Services
{
    public class WebsocketManager
    {
    private readonly ConcurrentDictionary<string, WebSocket> _connections = new ConcurrentDictionary<string, WebSocket>();

        public void AddConnection(string id, WebSocket webSocket)
        {
            _connections.TryAdd(id, webSocket);
        }

        public void RemoveConnection(string id)
        {
            _connections.TryRemove(id, out _);
        }

        public IEnumerable<WebSocket> GetAllConnections()
        {
            return _connections.Values;
        }

        public WebSocket GetConnection(string id)
        {
            _connections.TryGetValue(id, out var webSocket);
            return webSocket;
        }
    }
}