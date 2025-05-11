using Exiled.API.Interfaces;
using InventorySystem.Items.Firearms.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordJoinLeaveWebhook
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public string Webhook_url { get; set; } = "";
        public string Username_webhook { get; set; } = "";
        public bool Debug { get; set; } = false;
        public int Time_since_leaving_counter { get; set; } = 5;
        public string Scps_return_ping_role_id { get; set; } = "";
        public int Scps_return_alert_minutes { get; set; } = 2;
        public string Join_message { get; set; } = "joined the server.";
        public string Leave_message { get; set; } = "left the server.";
        public string Last_role { get; set; } = "Last role";
        public string Timer { get; set; } = "Last left the server {minutes} minutes ago.";

    }
}