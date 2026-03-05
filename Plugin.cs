using System;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using HarmonyLib;

namespace BroadcastStack
{
    public sealed class Plugin : Plugin<Config>
    {
        public override string Name => "BroadcastStack";
        public override string Author => "Artemis";
        public override Version Version => new Version(1, 0, 0);
        public override Version RequiredExiledVersion => new Version(9, 13, 1);

        internal static Plugin? Instance { get; private set; }

        private Harmony? _harmony;

        public override void OnEnabled()
        {
            Instance = this;
            BroadcastStacker.Init();

            Exiled.Events.Handlers.Player.Left += OnPlayerLeft;

            _harmony = new Harmony($"broadcaststack.{DateTime.UtcNow.Ticks}");
            _harmony.PatchAll();

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Player.Left -= OnPlayerLeft;

            _harmony?.UnpatchAll(_harmony.Id);
            _harmony = null;

            BroadcastStacker.Shutdown();
            Instance = null;

            base.OnDisabled();
        }

        private static void OnPlayerLeft(LeftEventArgs ev)
        {
            BroadcastStacker.ClearPlayer(ev.Player);
        }
    }
}
