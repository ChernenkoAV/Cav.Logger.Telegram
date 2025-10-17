using System.Text;
using Microsoft.Extensions.Logging;

namespace Cav.Logger.Telegram;

internal sealed class TelegramLogger(
    string categoryName,
    Func<TelegramLoggerConfiguration> getCurrentConfig,
    IExternalScopeProvider? scopeProvider) : ILogger
{
    private readonly string categoryName = categoryName;
    private readonly Func<TelegramLoggerConfiguration> getCurConfig = getCurrentConfig;

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

        var options = getCurConfig();

        var sb = new StringBuilder();

        sb.AppendLine($"{(options.UseEmoji ? toEmoji(logLevel) : String.Empty)}{logLevel} {categoryName}");

        Dictionary<string, string> adddata = [];

        scopeProvider?.ForEachScope((val, _) =>
        {
            if (val is null)
                return;

            var ikv = val as IEnumerable<KeyValuePair<string, object>>;
            if (ikv is not null)
                foreach (var item in ikv)
                    adddata[item.Key] = item.Value.ToString()!;
            else
                sb.AppendLine(val.ToString());
        }, state);

        if (exception is not null)
        {
            foreach (var key in exception.Data.Keys)
                if (key is not null)
                    adddata[key.ToString() ?? string.Empty] = exception.Data[key]?.ToString() ?? String.Empty;
        }

        foreach (var i in adddata)
            sb.AppendLine($"{i.Key}:{i.Value}");

        var formatedMessage = formatter?.Invoke(state, exception) ?? string.Empty;
        formatedMessage = formatedMessage.Trim(['\n', '\r']).Trim();
        if (!string.IsNullOrWhiteSpace(formatedMessage))
        {
            sb.AppendLine("-------");
            sb.AppendLine(formatedMessage);
        }

        if (exception is not null)
        {
            var printMessage = formatedMessage != exception.Message;

            if (printMessage)
            {
                sb.AppendLine("-------");
                sb.AppendLine(exception.Message);
            }

            if (options.ShowStackTrace?.Invoke(exception) ?? false)
            {
                if (!printMessage)
                    sb.AppendLine("-------");
                sb.AppendLine($"Type: {exception.GetType().FullName}");
                sb.AppendLine($"StackTrace: {exception.StackTrace}");
            }

            void vizitToInner(Exception? ex)
            {
                if (ex is null)
                    return;

                sb.AppendLine("----InnerException---");
                foreach (var key in ex.Data.Keys)
                    sb.AppendLine($"{key}: {ex.Data[key]}");
                sb.AppendLine("-------");
                sb.AppendLine(ex.Message);

                if (options.ShowStackTrace?.Invoke(ex) ?? false)
                {
                    sb.AppendLine($"Type: {ex.GetType().FullName}");
                    sb.AppendLine($"StackTrace: {exception.StackTrace}");
                }

                vizitToInner(ex.InnerException);
            }

            vizitToInner(exception.InnerException);
        }

        QueueMessageWriter.Enqueue(sb.ToString(), options.BotToken!, options.ChatId!, options.DisableNotification?.Invoke() ?? false);
    }

    /// <summary>
    /// Преобразование <see cref="LogLevel"/> в эмодзи
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    private string toEmoji(LogLevel level) =>
        level switch
        {
            LogLevel.Trace => "⚡️",
            LogLevel.Debug => "⚙️",
            LogLevel.Information => "ℹ️",
            LogLevel.Warning => "⚠️",
            LogLevel.Error => "🛑",
            LogLevel.Critical => "❌",
            _ => "💤"
        };
}
