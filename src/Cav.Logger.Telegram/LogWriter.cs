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

        var dn = queueMessage.DisableNotification.ToString().ToLower();

        if (queueMessage.Message.Length >= 4000)
        {
            req.Resource = "sendDocument";
            req.AlwaysMultipartFormData = true;

            req.AddParameter("file_id", DateTime.Now.Ticks.ToString())
                .AddParameter("chat_id", queueMessage.ChatId)
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
                chat_id = queueMessage.ChatId,
                text = queueMessage.Message,
                disable_notification = dn
            });
        }

        try
        {
            using var client = new RestClient(new RestClientOptions(new Uri($"https://api.telegram.org/bot{queueMessage.BotToken}/")));
            await client.ExecuteAsync(req).ConfigureAwait(false);
        }
        catch { }
    }
}
