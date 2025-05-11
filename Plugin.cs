using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordJoinLeaveWebhook
{
    public class Plugin : Plugin<Config>
    {
        public override string Name => "DiscordJoinLeaveWebhook";
        public override string Author => "Workertom";

        private EventHandlers _handlers;
        public static Plugin Instance { get; private set; }
        public static Dictionary<string, string> SteamToDiscord { get; private set; } = new Dictionary<string, string>();
        public override void OnEnabled()
        {

            Instance = this;
            base.OnEnabled();
            RegisterEvents();
            LoadRoleMappings();


        }
        public override void OnDisabled()
        {
            Instance = null;
            base.OnDisabled();
            UnregisterEvents();

        }
        private void LoadRoleMappings()
        {
            string path = Path.Combine(Paths.Configs, "DiscordJoinLeaveWebhook", "roles.txt");

            if (!File.Exists(path))
                return;

            foreach (string line in File.ReadLines(path))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                var parts = line.Split('@');
                if (parts.Length == 2)
                    SteamToDiscord[parts[0].Trim()] = parts[1].Trim();
            }
            Log.Debug($" Loaded {SteamToDiscord.Count} user mappings.");
        }
        private void RegisterEvents()
        {
            _handlers = new EventHandlers();
            Exiled.Events.Handlers.Player.Joined += _handlers.OnJoined;
            Exiled.Events.Handlers.Player.Left += _handlers.OnLeft;
            Exiled.Events.Handlers.Server.RoundEnded += _handlers.OnRoundEnded;
        }
        private void UnregisterEvents()
        {
            Exiled.Events.Handlers.Player.Joined -= _handlers.OnJoined;
            Exiled.Events.Handlers.Player.Left -= _handlers.OnLeft;
            Exiled.Events.Handlers.Server.RoundEnded -= _handlers.OnRoundEnded;
            _handlers = null;
        }
    }
}