using Exiled.API.Features;
using Exiled.API.Interfaces;
using Exiled.Events.EventArgs;
using Exiled.Events.EventArgs.Player;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SCPDiscord.EventListeners
{
    public class Plugin : Plugin<Config>
    {
        private static readonly HttpClient HttpClient = new HttpClient();
        private Dictionary<string, DateTime> lastLogoutTimes = new Dictionary<string, DateTime>();
        private Dictionary<string, string> lastRoles = new Dictionary<string, string>();

        public static Plugin Instance { get; private set; }

        public override void OnEnabled()
        {
            Instance = this;
            if (Config.Debug)
            {
                Exiled.API.Features.Log.Info($"Discord Webhook URL: {Config.WebhookUrl}");
            }

            Exiled.Events.Handlers.Player.Verified += OnVerified;
            Exiled.Events.Handlers.Player.Destroying += OnDestroying;
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Player.Verified -= OnVerified;
            Exiled.Events.Handlers.Player.Destroying -= OnDestroying;
            Instance = null;
            base.OnDisabled();
        }

        private async void OnVerified(VerifiedEventArgs ev)
        {
            if (Config.Debug)
            {
                Exiled.API.Features.Log.Info($"Player verified: {ev.Player.Nickname} ({ev.Player.UserId})");
            }

            if (string.IsNullOrEmpty(ev.Player.UserId))
            {
                if (Config.Debug)
                {
                    Exiled.API.Features.Log.Warn("Player UserId is null or empty. Skipping OnVerified.");
                }
                return;
            }

            string playerId = $"{ev.Player.Nickname} ({ev.Player.UserId} | {ev.Player.ReferenceHub.characterClassManager.connectionToClient.address})";
            string message = $"{playerId} влезе в сървъра.";

            if (lastLogoutTimes.TryGetValue(ev.Player.UserId, out DateTime lastLogoutTime))
            {
                TimeSpan timeSinceLastLogout = DateTime.Now - lastLogoutTime;
                if (timeSinceLastLogout.TotalMinutes <= 5)
                {
                    message += $" Последно излезе от сървъра преди {timeSinceLastLogout.Minutes} минути.";
                }
            }

            if (lastRoles.TryGetValue(ev.Player.UserId, out string lastRole))
            {
                message += $" Последна роля: {lastRole}.";
            }

            await SendWebhookMessageAsync(message);
        }

        private async void OnDestroying(DestroyingEventArgs ev)
        {
            if (Config.Debug)
            {
                Exiled.API.Features.Log.Info($"Player left: {ev.Player.Nickname} ({ev.Player.UserId})");
            }

            if (ev.Player?.UserId == null)
            {
                if (Config.Debug)
                {
                    Exiled.API.Features.Log.Warn("Player UserId is null on leave. Skipping OnDestroying.");
                }
                return;
            }

            string playerId = $"{ev.Player.Nickname} ({ev.Player.UserId} | {ev.Player.ReferenceHub.characterClassManager.connectionToClient.address})";
            string lastRole = ev.Player.Role.ToString();

            string message = $"{playerId} излезе от сървъра.\n";
            message += $"Последна роля в играта: {lastRole}.\n";

            lastLogoutTimes[ev.Player.UserId] = DateTime.Now;
            lastRoles[ev.Player.UserId] = lastRole;

            await SendWebhookMessageAsync(message);
        }

        private async Task SendWebhookMessageAsync(string message)
        {
            try
            {
                if (Config.Debug)
                {
                    Exiled.API.Features.Log.Info($"Sending webhook message: {message}");
                }

                var payload = new
                {
                    content = message
                };

                var json = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
                var data = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await HttpClient.PostAsync(Instance.Config.WebhookUrl, data);

                if (Config.Debug)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        Exiled.API.Features.Log.Info("Message sent successfully to Discord.");
                    }
                    else
                    {
                        Exiled.API.Features.Log.Warn($"Failed to send message to Discord. Status code: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                if (Config.Debug)
                {
                    Exiled.API.Features.Log.Error($"Error sending webhook message: {ex.Message}");
                }
            }
        }
    }

    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public string WebhookUrl { get; set; }
        public bool Debug { get; set; } = false;
    }
}
