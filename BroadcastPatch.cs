using System.Linq;
using System.Text.RegularExpressions;
using Exiled.API.Features;
using HarmonyLib;
using Mirror;
using RemoteAdmin;
using static Broadcast;

namespace BroadcastStack
{
    internal static class IdSanitizer
    {
        private static readonly Regex Pattern = new(@"\d{5,20}@\w+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static string Strip(string s) => string.IsNullOrEmpty(s) ? s : Pattern.Replace(s, string.Empty).Trim();
    }

    [HarmonyPatch(typeof(Player), nameof(Player.Broadcast), new[] { typeof(ushort), typeof(string), typeof(BroadcastFlags), typeof(bool) })]
    internal static class BroadcastPatch
    {
        private static bool Prefix(Player __instance, ushort duration, string message, BroadcastFlags type, bool shouldClearPrevious)
        {
            if (BroadcastStacker.IsInternalSend) return true;
            if (Plugin.Instance == null || !Plugin.Instance.Config.IsEnabled) return true;
            if (__instance == null || !__instance.IsVerified) return true;

            if (shouldClearPrevious && Plugin.Instance.Config.ClearOnShouldClearPrevious)
                BroadcastStacker.ClearPlayer(__instance);

            BroadcastStacker.AddToPlayer(__instance, duration, message);
            return false;
        }
    }

    [HarmonyPatch(typeof(Map), nameof(Map.Broadcast), new[] { typeof(ushort), typeof(string), typeof(BroadcastFlags), typeof(bool) })]
    internal static class MapBroadcastPatch
    {
        private static bool Prefix(ushort duration, string message, BroadcastFlags type, bool shouldClearPrevious)
        {
            if (BroadcastStacker.IsInternalSend) return true;
            if (Plugin.Instance == null || !Plugin.Instance.Config.IsEnabled) return true;

            if (shouldClearPrevious && Plugin.Instance.Config.ClearOnShouldClearPrevious)
                BroadcastStacker.ClearAll();

            BroadcastStacker.AddToAllPlayers(duration, message);
            return false;
        }
    }

    [HarmonyPatch(typeof(Broadcast), nameof(Broadcast.RpcAddElement), new[] { typeof(string), typeof(ushort), typeof(BroadcastFlags) })]
    internal static class NativeBroadcastRpcPatch
    {
        private static bool Prefix(string data, ushort time, BroadcastFlags flags)
        {
            if (BroadcastStacker.IsInternalSend) return true;
            if (Plugin.Instance == null || !Plugin.Instance.Config.IsEnabled) return true;

            BroadcastStacker.AddToAllPlayers(time, data);
            return false;
        }
    }

    [HarmonyPatch(typeof(Broadcast), nameof(Broadcast.TargetAddElement), new[] { typeof(NetworkConnection), typeof(string), typeof(ushort), typeof(BroadcastFlags) })]
    internal static class NativeBroadcastTargetPatch
    {
        private static bool Prefix(NetworkConnection conn, string data, ushort time, BroadcastFlags flags)
        {
            if (BroadcastStacker.IsInternalSend) return true;
            if (Plugin.Instance == null || !Plugin.Instance.Config.IsEnabled) return true;
            if (conn == null) return true;

            var pl = Player.List.FirstOrDefault(p => p != null && p.IsVerified && p.Connection == conn);
            if (pl == null) return true;

            BroadcastStacker.AddToPlayer(pl, time, data);
            return false;
        }
    }

    [HarmonyPatch(typeof(CommandProcessor), "ProcessAdminChat")]
    internal static class AdminChatPatch
    {
        private static void Postfix(string q, CommandSender sender)
        {
            if (BroadcastStacker.IsInternalSend) return;
            if (Plugin.Instance == null || !Plugin.Instance.Config.IsEnabled) return;

            var cfg = Plugin.Instance.Config;
            if (!cfg.StackStaffChat) return;

            var msg = IdSanitizer.Strip(q);
            if (string.IsNullOrWhiteSpace(msg)) return;

            var player = Player.Get(sender);
            var label = cfg.ShowStaffChatSenderName
                ? $"{cfg.StaffChatPrefix} {(player != null ? player.DisplayNickname : "Console")}"
                : cfg.StaffChatPrefix;

            var broadcast = $"{label}: {msg}";

            foreach (var p in Player.List)
            {
                if (p == null || !p.IsVerified) continue;
                if (p.ReferenceHub?.serverRoles?.RemoteAdmin != true) continue;
                BroadcastStacker.AddToPlayer(p, cfg.StaffChatDuration, broadcast);
            }
        }
    }
}
