using System.Text;
using Microsoft.Extensions.Logging;

namespace Cav.Logger.Telegram;

/// <summary>
/// Форматер сообщения. 
/// </summary>
internal class TelegramMessageFormatter
{
    public virtual string Format(
        LogLevel logLevel,
        string category,
        string message,
        Exception? exception,
        TelegramLoggerConfiguration options)
    {
        if (string.IsNullOrWhiteSpace(message))
            return string.Empty;

        var sb = new StringBuilder();
#pragma warning disable CA1305 // Укажите IFormatProvider
        sb.AppendLine($"{(options.UseEmoji ? ToEmoji(logLevel) : String.Empty)}{logLevel} {category}");
#pragma warning restore CA1305 // Укажите IFormatProvider

        sb.Append(message);

        if (exception != null)
        {
            sb.AppendLine();
            sb.AppendLine(exception.ToString());
        }

        return sb.ToString();
    }

    /// <summary>
    /// Преобразование <see cref="LogLevel"/> в эмодзи
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    public virtual string ToEmoji(LogLevel level) =>
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