using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cav.Logger.Telegram;

[ProviderAlias("Telegram")]
internal sealed class TelegramLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private readonly IDisposable? onChangeToken;
    private TelegramLoggerConfiguration curConfig;
    private readonly ConcurrentDictionary<string, TelegramLogger> loggers =
        new(StringComparer.OrdinalIgnoreCase);

    private IExternalScopeProvider? iExternalScopeProvider;
    public TelegramLoggerProvider(
        IOptionsMonitor<TelegramLoggerConfiguration> config)
    {
        curConfig = config.CurrentValue;
        onChangeToken = config.OnChange(updatedConfig => curConfig = updatedConfig);
    }

    public ILogger CreateLogger(string categoryName) =>
        loggers.GetOrAdd(categoryName, name => new TelegramLogger(name, getCurrentConfig, iExternalScopeProvider));

    private TelegramLoggerConfiguration getCurrentConfig() => curConfig;

    public void Dispose()
    {
        loggers.Clear();
        onChangeToken?.Dispose();
    }

    public void SetScopeProvider(IExternalScopeProvider scopeProvider) => iExternalScopeProvider = scopeProvider;
}
