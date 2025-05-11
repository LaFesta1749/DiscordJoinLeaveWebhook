using Discord;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordJoinLeaveWebhook
{
    public class EventHandlers
    {
        private static readonly Dictionary<string, DateTime> _lastDisconnectTime = new Dictionary<string, DateTime>();
        private static readonly Dictionary<string, RoleTypeId> _lastValidRole = new Dictionary<string, RoleTypeId>();

        private static bool WasSCP(RoleTypeId role) =>
            role.ToString().StartsWith("Scp", StringComparison.OrdinalIgnoreCase);
        public void OnJoined(JoinedEventArgs ev)
        {
            string userID = ev.Player.UserId;
            string nickname = ev.Player.Nickname;
            RoleTypeId currentRole = ev.Player.Role;

            string message = $"{nickname} ({userID}) {Plugin.Instance.Config.Join_message}";

            if (Round.IsLobby || Round.IsEnded)
            {
                Log.Debug("the round is being restarted or is in lobby");
                return;
            }
            if (_lastDisconnectTime.TryGetValue(userID, out var lastTime))
            {
                var minutesSinceLeave = (DateTime.UtcNow - lastTime).TotalMinutes;

                if (minutesSinceLeave <= Plugin.Instance.Config.Time_since_leaving_counter)
                {
                    message += $"\n{Plugin.Instance.Config.Timer.Replace("{minutes}", Math.Round(minutesSinceLeave).ToString())}";

                    if (_lastValidRole.TryGetValue(userID, out var lastRole) && WasSCP(lastRole))
                    {
                        if (minutesSinceLeave <= Plugin.Instance.Config.Scps_return_alert_minutes)
                        {
                            message += $"\n🔔 <@&{Plugin.Instance.Config.Scps_return_ping_role_id}> | Player {nickname} was previously **{lastRole}** and rejoined quickly!";
                        }
                    }
                    Log.Debug($"{nickname} rejoined after {Math.Round(minutesSinceLeave)} min (last role: {lastRole})");
                }
                else
                {
                    _lastDisconnectTime.Remove(userID);
                    _lastValidRole.Remove(userID);

                    Log.Debug($"Removed old data for {nickname} ({userID})");
                }

            }
            Log.Debug($"Sending join webhook: {message}");

            _ = WebhookHandler.SendWebhookMessageAsync(message);
        }

        public void OnLeft(LeftEventArgs ev)
        {
            if (ev?.Player == null)
            {
                Log.Debug("Player is null. Skipping leave webhook.");
                return;
            }

            string userID = ev.Player.UserId;
            string nickname = ev.Player.Nickname;

            if (string.IsNullOrEmpty(userID))
            {
                Log.Debug("Player.UserId is null or empty. Skipping leave webhook.");
                return;
            }

            if (Round.IsLobby || Round.IsEnded)
            {
                Log.Debug($"Player {nickname} left during lobby or round restart. Ignoring.");
                return;
            }

            RoleTypeId lastRole = ev.Player.Role;
            _lastDisconnectTime[userID] = DateTime.UtcNow;
            _lastValidRole[userID] = lastRole;

            string message = $"{nickname} ({userID}) {Plugin.Instance.Config.Leave_message}\n{Plugin.Instance.Config.Last_role}: {lastRole}";

            Log.Debug($"{nickname} left as {lastRole} at {_lastDisconnectTime[userID]}");
            Log.Debug($"Sending leave webhook: {message}");

            _ = WebhookHandler.SendWebhookMessageAsync(message);
        }


        public void OnRoundEnded(RoundEndedEventArgs ev)
        {
            _lastDisconnectTime.Clear();
            _lastValidRole.Clear();
            Log.Debug("Cleared disconnect tracking data after round ended.");
        }
    }
}