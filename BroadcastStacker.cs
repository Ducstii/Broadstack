using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using MEC;

namespace BroadcastStack
{
    internal static class BroadcastStacker
    {
        [ThreadStatic]
        internal static bool IsInternalSend;

        private sealed class Entry
        {
            public Guid Id;
            public string Content = string.Empty;
            public float ExpireAt;
        }

        private static bool _enabled;
        private static readonly Dictionary<string, Dictionary<Guid, Entry>> PlayerStacks = new();
        private static readonly Dictionary<string, uint> PlayerGeneration = new();
        private static uint _globalGeneration;

        internal static void Init() => _enabled = true;

        internal static void Shutdown()
        {
            _enabled = false;
            PlayerStacks.Clear();
            PlayerGeneration.Clear();
            _globalGeneration++;
        }

        public static void ClearPlayer(Player player)
        {
            if (player == null) return;
            PlayerStacks.Remove(player.UserId);
            PlayerGeneration[player.UserId] = PlayerGeneration.TryGetValue(player.UserId, out var g) ? g + 1 : 1;
            if (_enabled && player.IsConnected) Refresh(player);
        }

        public static void ClearAll()
        {
            PlayerStacks.Clear();
            _globalGeneration++;
            if (_enabled)
            {
                foreach (var p in Player.List)
                {
                    if (p != null && p.IsVerified && p.IsConnected) Refresh(p);
                }
            }
        }

        public static void AddToAllPlayers(ushort duration, string message)
        {
            foreach (var p in Player.List)
            {
                if (p == null || !p.IsVerified) continue;
                AddToPlayer(p, duration, message);
            }
        }

        public static void AddToPlayer(Player player, ushort duration, string message)
        {
            if (!_enabled || player == null || !player.IsVerified) return;

            var cfg = Plugin.Instance?.Config;
            if (cfg == null || duration == 0) return;

            message = Normalize(message);
            if (cfg.IgnoreEmpty && string.IsNullOrWhiteSpace(message)) return;

            if (!PlayerStacks.TryGetValue(player.UserId, out var stack))
                PlayerStacks[player.UserId] = stack = new Dictionary<Guid, Entry>();

            var id = Guid.NewGuid();
            stack[id] = new Entry { Id = id, Content = message, ExpireAt = Timing.LocalTime + duration };

            Refresh(player);

            var userId = player.UserId;
            PlayerGeneration.TryGetValue(userId, out var gen);
            var globalGen = _globalGeneration;

            Timing.CallDelayed(duration, () =>
            {
                PlayerGeneration.TryGetValue(userId, out var currentGen);
                if (currentGen != gen || _globalGeneration != globalGen) return;

                if (!PlayerStacks.TryGetValue(userId, out var s)) return;
                s.Remove(id);
                if (s.Count == 0) PlayerStacks.Remove(userId);
                if (player.IsConnected) Refresh(player);
            });
        }

        private static void Refresh(Player player)
        {
            if (!PlayerStacks.TryGetValue(player.UserId, out var stack))
            {
                Send(player, 1, string.Empty);
                return;
            }

            float now = Timing.LocalTime;
            var active = stack.Values.Where(e => e.ExpireAt > now).OrderByDescending(e => e.ExpireAt).ToList();

            if (active.Count == 0)
            {
                PlayerStacks.Remove(player.UserId);
                Send(player, 1, string.Empty);
                return;
            }

            float minLeft = active.Min(e => e.ExpireAt - now);
            if (minLeft < 1f) minLeft = 1f;

            Send(player, (ushort)Math.Min(300, Math.Ceiling(minLeft)),
                string.Join("\n", active.Select(e => Normalize(e.Content)).Where(s => !string.IsNullOrWhiteSpace(s))));
        }

        private static void Send(Player player, ushort duration, string text)
        {
            try
            {
                IsInternalSend = true;
                player.Broadcast(duration, text, Plugin.Instance!.Config.Flags, true);
            }
            finally
            {
                IsInternalSend = false;
            }
        }

        private static string Normalize(string msg) =>
            (msg ?? string.Empty).Replace("\r", string.Empty).TrimEnd('\n');
    }
}
