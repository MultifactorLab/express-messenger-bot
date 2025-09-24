using System.ComponentModel.DataAnnotations;

namespace MF.Express.Bot.Infrastructure.Configuration;

/// <summary>
/// Конфигурация для интеграции с Multifactor API
/// </summary>
public class MultifactorApiConfiguration
{
    public const string SectionName = "MultifactorApi";

    /// <summary>
    /// Базовый URL Multifactor API
    /// </summary>
    [Required]
    public string BaseUrl { get; set; } = default!;

    /// <summary>
    /// API ключ для аутентификации
    /// </summary>
    [Required]
    public string ApiKey { get; set; } = default!;

    /// <summary>
    /// Таймаут запросов в секундах
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Количество попыток повторных запросов
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Endpoint для отправки информации о пользователе/чате
    /// </summary>
    public string UserChatInfoEndpoint { get; set; } = "/api/bot/user-chat-info";

    /// <summary>
    /// Endpoint для отправки результата авторизации
    /// </summary>
    public string AuthorizationResultEndpoint { get; set; } = "/api/bot/authorization-result";
}
