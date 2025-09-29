using MF.Express.Bot.Application.DTOs;

namespace MF.Express.Bot.Application.Interfaces;

/// <summary>
/// Сервис для взаимодействия с Multifactor API
/// </summary>
public interface IMultifactorApiService
{
    /// <summary>
    /// Отправляет информацию о пользователе и чате в Multifactor API
    /// </summary>
    Task<bool> SendUserChatInfoAsync(UserChatInfoDto userChatInfo, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Отправляет результат авторизации в Multifactor API
    /// </summary>
    Task<bool> SendAuthorizationResultAsync(AuthorizationResultDto authResult, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получает дополнительную информацию о пользователе из Multifactor API (если нужно)
    /// </summary>
    Task<UserInfoResponse?> GetUserInfoAsync(string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Отправляет полные данные пользователя в Multifactor API при команде /start
    /// </summary>
    Task<bool> SendUserStartCommandDataAsync(UserStartCommandDataDto userData, CancellationToken cancellationToken = default);
}

/// <summary>
/// Ответ с информацией о пользователе из Multifactor API
/// </summary>
public record UserInfoResponse(
    string UserId,
    string? Email,
    string? DisplayName,
    bool IsActive,
    Dictionary<string, object>? AdditionalData = null
);
