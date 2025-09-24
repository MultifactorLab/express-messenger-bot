using System.ComponentModel.DataAnnotations;

namespace MF.Express.Bot.Infrastructure.Configuration;

/// <summary>
/// Конфигурация Express Bot для Bot API v4 интеграции
/// </summary>
public class ExpressBotConfiguration
{
    public const string SectionName = "ExpressBot";

    /// <summary>
    /// ID бота в системе Express (UUID)
    /// </summary>
    [Required]
    public string BotId { get; set; } = string.Empty;

    /// <summary>
    /// Секретный ключ бота для валидации JWT токенов от BotX
    /// </summary>
    [Required]
    public string BotSecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Базовый URL BotX API для отправки сообщений
    /// </summary>
    [Required]
    public string BotXApiBaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Ожидаемый issuer в JWT токенах (хост BotX сервера)
    /// </summary>
    [Required]
    public string ExpectedIssuer { get; set; } = string.Empty;

    /// <summary>
    /// Таймаут для HTTP запросов к BotX API (секунды)
    /// </summary>
    public int RequestTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Версия протокола Bot API (по умолчанию 4)
    /// </summary>
    public int ProtoVersion { get; set; } = 4;

    /// <summary>
    /// Включить подробное логирование Bot API запросов
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = false;
}

