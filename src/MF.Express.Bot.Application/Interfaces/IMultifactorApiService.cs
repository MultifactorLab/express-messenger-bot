using MF.Express.Bot.Application.Models.Auth;
using MF.Express.Bot.Application.Models.BotCommand;

namespace MF.Express.Bot.Application.Interfaces;

/// <summary>
/// Сервис для взаимодействия с Multifactor API
/// </summary>
public interface IMultifactorApiService
{
    Task<bool> SendUserChatInfoAsync(UserChatInfoAppModel userChatInfo, CancellationToken cancellationToken = default);
    
    Task<bool> SendAuthorizationResultAsync(AuthorizationResultAppModel authResult, CancellationToken cancellationToken = default);
    
    Task<UserInfoResponse?> GetUserInfoAsync(string userId, CancellationToken cancellationToken = default);
    
    Task<bool> SendUserStartCommandDataAsync(UserStartCommandAppModel userData, CancellationToken cancellationToken = default);
}

public record UserInfoResponse(
    string UserId,
    string? Email,
    string? DisplayName,
    bool IsActive,
    Dictionary<string, object>? AdditionalData = null
);
