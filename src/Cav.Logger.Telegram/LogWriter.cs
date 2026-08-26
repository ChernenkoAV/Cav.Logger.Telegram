using System.Diagnostics;
using System.Net;
using System.Text;
using RestSharp;

namespace Cav.Logger.Telegram;

internal static class TelegramLogWriter
{
    public static async Task Write(QueueMessage queueMessage)
    {
        var req = new RestRequest()
        {
            Method = Method.Post
        };

        var dn = queueMessage.Settings.DisableNotification.ToString().ToLower();

        if (queueMessage.Message.Length >= 4000)
        {
            req.Resource = "sendDocument";
            req.AlwaysMultipartFormData = true;

            req.AddParameter("file_id", DateTime.Now.Ticks.ToString())
                .AddParameter("chat_id", queueMessage.Settings.ChatId)
                .AddParameter("caption", queueMessage.Message[..1000])
                .AddParameter("disable_content_type_detection", "true")
                .AddParameter("disable_notification", dn)
                .AddFile("document", Encoding.UTF8.GetBytes(queueMessage.Message), "error.txt");
        }
        else
        {
            req.Resource = "sendMessage";
            req.AddJsonBody(new
            {
                chat_id = queueMessage.Settings.ChatId,
                text = queueMessage.Message,
                disable_notification = dn
            });
        }

        var uriBuilder = new UriBuilder(queueMessage.Settings.Relay ?? new Uri("https://api.telegram.org"));
        uriBuilder.Path = $"{uriBuilder.Path}/bot{queueMessage.Settings.BotToken}".Trim('/');

        var webProxy = queueMessage.Settings.Proxy is null ? null : new WebProxy(queueMessage.Settings.Proxy);

        try
        {
            using var client = new RestClient(new RestClientOptions(uriBuilder.Uri) { Proxy = webProxy }, useClientFactory: true);
            await client.ExecuteAsync(req).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex.Message);
        }
    }
}
