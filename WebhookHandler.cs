using Exiled.API.Features;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DiscordJoinLeaveWebhook
{
    public static class WebhookHandler
    {
        private static readonly HttpClient Client = new HttpClient();
        private static readonly string WebhookUrl = Plugin.Instance.Config.Webhook_url;

        public static async Task SendWebhookMessageAsync(string message)
        {
            var successWebhook = new
            {
                username = Plugin.Instance.Config.Username_webhook,
                content = message,

            };
            string jsonContent = Utf8Json.JsonSerializer.ToJsonString(successWebhook);
            StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage responseMessage = await Client.PostAsync(WebhookUrl, content);
                string responseMessageString = await responseMessage.Content.ReadAsStringAsync();

                if (!responseMessage.IsSuccessStatusCode)
                {
                    Log.Error($"[{(int)responseMessage.StatusCode} - {responseMessage.StatusCode}] A non-successful status code was returned by Discord when trying to post to webhook. Response Message: {responseMessageString} .");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error sending webhook message: {ex.Message}");
            }
        }
    }
}