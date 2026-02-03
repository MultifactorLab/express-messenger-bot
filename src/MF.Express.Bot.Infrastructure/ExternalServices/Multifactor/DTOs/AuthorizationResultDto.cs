using MF.Express.Bot.Application.Models.Auth;

namespace MF.Express.Bot.Infrastructure.ExternalServices.Multifactor.DTOs;

public record AuthorizationResultDto(
    string AuthRequestId,
    string UserId,
    int Action,
    DateTime ProcessedAt,
    string? AdditionalInfo = null
)
{
    public static AuthorizationResultDto FromAppModel(AuthorizationResultAppModel model)
    {
        return new AuthorizationResultDto(
            AuthRequestId: model.AuthRequestId,
            UserId: model.UserId,
            Action: (int)model.Action,
            ProcessedAt: model.ProcessedAt,
            AdditionalInfo: model.AdditionalInfo
        );
    }
}

