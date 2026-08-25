namespace Cav.Logger.Telegram;
/// <summary>
/// Настройки логгера
/// </summary>
public sealed class TelegramLoggerConfiguration
{
    /// <summary>
    /// токен бота
    /// </summary>
    public string? BotToken { get; set; }
    /// <summary>
    /// Ид Чата
    /// </summary>
    public string? ChatId { get; set; }
    /// <summary>
    /// Всовывать эмоджи первым символом сообщения. Включено по умолчанию
    /// </summary>
    public bool UseEmoji { get; set; } = true;
    /// <summary>
    /// Прокси
    /// </summary>
    public Uri? Proxy { get; set; }
    /// <summary>
    /// Релейный сервер
    /// </summary>
    public Uri? Relay { get; set; }

    /// <summary>
    /// Функтор указания отключения уведомления для сообщения. По умолчанию - уведомление включено.
    /// </summary>
    /// <returns></returns>
    public Func<bool> DisableNotification { get; set; } = () => false;
    /// <summary>
    /// Функтор указания вывода в тексе <see cref="Exception.StackTrace"/>. По умолчанию - вывод отключен.
    /// </summary>
    public Func<Exception, bool> ShowStackTrace { get; set; } = (ex) => false;
}
