using System.Text.Json.Serialization;
using MF.Express.Bot.Application.Commands;

namespace MF.Express.Bot.Api.DTOs.SendAuthRequest;

public record SendAuthRequestDto(
    [property: JsonPropertyName("chat_id")] string ChatId,
    [property: JsonPropertyName("user_id")] string UserId,
    [property: JsonPropertyName("auth_request_id")] string AuthRequestId,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("resource_name")] string? ResourceName = null,
    [property: JsonPropertyName("metadata")] Dictionary<string, object>? Metadata = null
)
{
    public static SendAuthRequestCommand ToCommand(SendAuthRequestDto dto)
    {
        return new SendAuthRequestCommand(
            ChatId: dto.ChatId,
            UserId: dto.UserId,
            AuthRequestId: dto.AuthRequestId,
            Message: dto.Message,
            ResourceName: dto.ResourceName,
            Metadata: dto.Metadata
        );
    }
}

