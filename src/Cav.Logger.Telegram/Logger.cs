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

        return true;

        //return logLevel != LogLevel.None &&
        //    !String.IsNullOrWhiteSpace(curConf.BotToken) &&
        //    !String.IsNullOrWhiteSpace(curConf.ChatId);
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
        scopeProvider?.ForEachScope((val, _) =>
        {
            if (val is null)
                return;

            var ikv = val as IEnumerable<KeyValuePair<string, object>>;
            if (ikv is not null)
                foreach (var item in ikv)
                    sb.AppendLine($"{item.Key}:{item.Value}");
            else
                sb.AppendLine(val.ToString());
        }, state);

        if (exception is not null)
        {
            foreach (var key in exception.Data.Keys)
                sb.AppendLine($"{key}: {exception.Data[key]}");
        }

        var formatedMessage = formatter?.Invoke(state, exception) ?? string.Empty;
        formatedMessage = formatedMessage.TrimEnd(['\n', '\r']).Trim();
        if (!string.IsNullOrWhiteSpace(formatedMessage))
        {
            sb.AppendLine("-------");
            sb.AppendLine(formatedMessage);
        }

        if (exception is not null)
        {
            sb.AppendLine("-------");

            sb.AppendLine(exception.Message);
            sb.AppendLine($"Type: {exception.GetType().FullName}");
            if (options.ShowStackTrace?.Invoke(exception) ?? false)
                sb.AppendLine($"StackTrace: {exception.StackTrace}");

            void vizitToInner(Exception? ex)
            {
                if (ex is null)
                    return;

                sb.AppendLine("----InnerException---");
                sb.AppendLine(ex.Message);
                foreach (var key in ex.Data.Keys)
                    sb.AppendLine($"{key}: {ex.Data[key]}");
                sb.AppendLine($"Type: {ex.GetType().FullName}");

                if (options.ShowStackTrace?.Invoke(ex) ?? false)
                    sb.AppendLine($"StackTrace: {exception.StackTrace}");

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
