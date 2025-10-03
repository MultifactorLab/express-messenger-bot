using MF.Express.Bot.Application.Models.Auth;

namespace MF.Express.Bot.Infrastructure.ExternalServices.Multifactor.DTOs;

public record UserChatInfoDto(
    string UserId,
    string ChatId,
    string? Username,
    string? FirstName,
    string? LastName,
    DateTime FirstContactTime,
    string? LastMessage = null,
    Dictionary<string, object>? Metadata = null
)
{
    public static UserChatInfoDto FromAppModel(UserChatInfoAppModel model)
    {
        return new UserChatInfoDto(
            UserId: model.UserId,
            ChatId: model.ChatId,
            Username: model.Username,
            FirstName: model.FirstName,
            LastName: model.LastName,
            FirstContactTime: model.FirstContactTime,
            LastMessage: model.LastMessage,
            Metadata: model.Metadata
        );
    }
}

