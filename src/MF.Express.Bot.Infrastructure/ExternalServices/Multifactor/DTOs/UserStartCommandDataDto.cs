using MF.Express.Bot.Application.Models.BotCommand;

namespace MF.Express.Bot.Infrastructure.ExternalServices.Multifactor.DTOs;

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
)
{
    public static UserStartCommandDataDto FromAppModel(UserStartCommandAppModel model)
    {
        return new UserStartCommandDataDto(
            UserId: model.UserId,
            ChatId: model.ChatId,
            Username: model.Username,
            FirstName: model.FirstName,
            LastName: model.LastName,
            AdLogin: model.AdLogin,
            AdDomain: model.AdDomain,
            ChatType: model.ChatType,
            Timestamp: model.Timestamp,
            Platform: model.Platform,
            AppVersion: model.AppVersion,
            Device: model.Device,
            Locale: model.Locale,
            Metadata: model.Metadata
        );
    }
}

