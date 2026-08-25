using System.Collections.Concurrent;

namespace Cav.Logger.Telegram;

internal record QueueMessageSettings(string BotToken, string ChatId, bool DisableNotification, Uri? Proxy, Uri? Relay);
internal record QueueMessage(string Message, QueueMessageSettings Settings);

internal static class QueueMessageWriter
{
    private static BlockingCollection<QueueMessage> queues = [];

    static QueueMessageWriter()
    {
        var thrd = new Thread(sendMesg)
        {
            IsBackground = true,
            Name = "Telegram sender thread"
        };

        thrd.Start();
    }

    public static void Enqueue(string message, QueueMessageSettings settings) =>
        queues.Add(new(message, settings));

    private static void sendMesg()
    {
        foreach (var qm in queues.GetConsumingEnumerable())
            try
            {
                TelegramLogWriter.Write(qm).GetAwaiter().GetResult();
            }
            finally
            {
            }
    }
}
