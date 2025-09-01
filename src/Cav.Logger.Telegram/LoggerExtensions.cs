using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace Cav.Logger.Telegram;

/// <summary>
/// Расширения для добавления логера
/// </summary>
public static class TelegramLoggerExtensions
{
    /// <summary>
    /// Добавить логгер
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static ILoggingBuilder AddTelegramLogger(
        this ILoggingBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddConfiguration();

        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, TelegramLoggerProvider>());

        LoggerProviderOptions.RegisterProviderOptions<TelegramLoggerConfiguration, TelegramLoggerProvider>(builder.Services);

        return builder;
    }

    /// <summary>
    /// Добавить логгер с коррекцией настроек
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static ILoggingBuilder AddTelegramLogger(
        this ILoggingBuilder builder,
        Action<TelegramLoggerConfiguration> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);

        builder.AddTelegramLogger();
        builder.Services.Configure(configure);

        return builder;
    }
}
