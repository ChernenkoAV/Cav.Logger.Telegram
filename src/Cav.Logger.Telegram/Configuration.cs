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
    /// Функтор указания отключения уведомления для сообщения. 
    /// </summary>
    /// <returns></returns>
    public Func<bool> DisableNotification { get; set; } = () => false;
}
