using Microsoft.Extensions.Logging;

namespace Cav.Logger.Telegram;

internal sealed class TelegramLogger(
    string categoryName,
    Func<TelegramLoggerConfiguration> getCurrentConfig) : ILogger, IDisposable
{
    private readonly string categoryName = categoryName;
    private readonly Func<TelegramLoggerConfiguration> getCurConfig = getCurrentConfig;

    private QueueMessageWriter qmWriter = new();
    private TelegramMessageFormatter telMesFormatter = new();

#pragma warning disable IDE0060 // Удалите неиспользуемый параметр
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;
#pragma warning restore IDE0060 // Удалите неиспользуемый параметр

    public bool IsEnabled(LogLevel logLevel)
    {
        var curConf = getCurConfig();

        return logLevel != LogLevel.None &&
            !String.IsNullOrWhiteSpace(curConf.BotToken) &&
            !String.IsNullOrWhiteSpace(curConf.ChatId);
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        string message = null!;
        if (formatter != null)
            message = formatter(state, exception);

        if (exception != null)
        {
            if (@String.IsNullOrWhiteSpace(message))
                message += Environment.NewLine;

            message += exception.Message;
        }

        var options = getCurConfig();

        if (!String.IsNullOrWhiteSpace(message))
            message = telMesFormatter.Format(logLevel, categoryName, message, exception, options);

        if (!String.IsNullOrWhiteSpace(message))
            qmWriter.Enqueue(message, options.BotToken!, options.ChatId!, options.DisableNotification?.Invoke() ?? false);
    }

    public void Dispose() => qmWriter.Dispose();
}
