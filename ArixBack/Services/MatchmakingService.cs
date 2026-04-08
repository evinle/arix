using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using ArixBack.Models;

namespace ArixBack.Services
{
    public class MatchmakingService
    {
        private readonly ConcurrentQueue<PlayerQueueEntry> _queue = new ConcurrentQueue<PlayerQueueEntry>();
        private readonly ConcurrentDictionary<string, PlayerQueueEntry> _playersInQueue = new ConcurrentDictionary<string, PlayerQueueEntry>();

        public void Enqueue(Player player, WebSocket socket)
        {
            if (_playersInQueue.ContainsKey(player.Id))
            {
                return;
            }

            var entry = new PlayerQueueEntry
            {
                Player = player,
                Socket = socket,
                JoinedAt = DateTime.UtcNow
            };

            _queue.Enqueue(entry);
            _playersInQueue.TryAdd(player.Id, entry);
        }

        public void Dequeue(string playerId)
        {
            _playersInQueue.TryRemove(playerId, out _);
        }

        public List<PlayerQueueEntry> GetQueue()
        {
            // Filter out players who are no longer in the dictionary (already matched or left)
            return _queue.Where(e => _playersInQueue.ContainsKey(e.Player.Id)).ToList();
        }

        public int GetUniquePlayerCount()
        {
            return _playersInQueue.Keys.Count;
        }

        public void ClearPlayer(string playerId)
        {
            _playersInQueue.TryRemove(playerId, out _);
        }
    }

    public class PlayerQueueEntry
    {
        public Player Player { get; set; }
        public WebSocket Socket { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}
