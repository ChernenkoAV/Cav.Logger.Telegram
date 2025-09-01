using System.Collections.Concurrent;

namespace Cav.Logger.Telegram;

internal class QueueMessage(string message, string botToken, string chatId, bool disableNotification)
{
    public string Message => message;
    public string BotToken => botToken;
    public string ChatId => chatId;
    public bool DisableNotification => disableNotification;
};

internal class QueueMessageWriter : IDisposable
{
    private BlockingCollection<QueueMessage> queues = [];

    public QueueMessageWriter()
    {
        var thrd = new Thread(sendMesg)
        {
            IsBackground = true,
            Name = "Telegram sender thread"
        };

        thrd.Start();
    }

    public void Enqueue(string message, string botToken, string chatId, bool disableNotification) =>
        queues.Add(new(message, botToken, chatId, disableNotification));

    private void sendMesg()
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

    public void Dispose()
    {
        queues.Dispose();
        GC.SuppressFinalize(this);
    }
}
