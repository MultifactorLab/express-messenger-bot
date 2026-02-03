using System.Text.Json.Serialization;
using MF.Express.Bot.Application.UseCases.Auth;

namespace MF.Express.Bot.Api.DTOs.SendAuthResult;

public record SendAuthResultDto(
    [property: JsonPropertyName("chat_id")] string ChatId,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("success")] bool Success
)
{
    public static SendAuthResultRequest ToRequest(SendAuthResultDto dto)
    {
        return new SendAuthResultRequest(
            ChatId: dto.ChatId,
            Message: dto.Message,
            Success: dto.Success
        );
    }
}


