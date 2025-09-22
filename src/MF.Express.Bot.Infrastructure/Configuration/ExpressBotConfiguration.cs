namespace MF.Express.Bot.Infrastructure.Configuration;

/// <summary>
/// Конфигурация Express Bot
/// </summary>
public class ExpressBotConfiguration
{
    public const string SectionName = "ExpressBot";

    /// <summary>
    /// Токен бота
    /// </summary>
    public string BotToken { get; set; } = string.Empty;

    /// <summary>
    /// Базовый URL Express API
    /// </summary>
    public string ApiBaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// URL webhook для обратных вызовов
    /// </summary>
    public string WebhookUrl { get; set; } = string.Empty;

    /// <summary>
    /// Секретный ключ для валидации webhook
    /// </summary>
    public string? WebhookSecret { get; set; }

    /// <summary>
    /// Таймаут для HTTP запросов (секунды)
    /// </summary>
    public int RequestTimeoutSeconds { get; set; } = 30;
}

