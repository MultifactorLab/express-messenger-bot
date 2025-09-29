namespace MF.Express.Bot.Application.DTOs;

/// <summary>
/// DTO для callback от кнопок авторизации
/// </summary>
public record AuthCallbackDto(
    string CallbackId,
    string UserId,
    string ChatId,
    string AuthRequestId,
    AuthAction Action,
    DateTime Timestamp,
    string? MessageId = null,
    Dictionary<string, object>? Metadata = null
);

/// <summary>
/// Действия пользователя по авторизации
/// </summary>
public enum AuthAction
{
    /// <summary>
    /// Пользователь разрешил авторизацию
    /// </summary>
    Allow = 1,
    
    /// <summary>
    /// Пользователь отклонил авторизацию
    /// </summary>
    Deny = 2
}

/// <summary>
/// DTO для отправки данных о пользователе и чате в MF API
/// </summary>
public record UserChatInfoDto(
    string UserId,
    string ChatId,
    string? Username,
    string? FirstName,
    string? LastName,
    DateTime FirstContactTime,
    string? LastMessage = null,
    Dictionary<string, object>? Metadata = null
);

/// <summary>
/// DTO для отправки результата авторизации в MF API  
/// </summary>
public record AuthorizationResultDto(
    string AuthRequestId,
    string UserId,
    AuthAction Action,
    DateTime ProcessedAt,
    string? AdditionalInfo = null
);

/// <summary>
/// DTO для отправки полных данных пользователя в Multifactor API при команде /start
/// </summary>
public record UserStartCommandDataDto(
    string UserId,
    string ChatId,
    string? Username,
    string? FirstName,
    string? LastName,
    string? AdLogin,
    string? AdDomain,
    string? ChatType,
    DateTime Timestamp,
    string? Platform,
    string? AppVersion,
    string? Device,
    string? Locale,
    Dictionary<string, object>? Metadata = null
);
