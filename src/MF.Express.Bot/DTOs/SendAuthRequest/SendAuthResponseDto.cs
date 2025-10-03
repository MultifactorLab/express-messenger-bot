using System.Text.Json.Serialization;
using MF.Express.Bot.Application.Models.SendAuthRequest;

namespace MF.Express.Bot.Api.DTOs.SendAuthRequest;

public record SendAuthResponseDto(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("message_id")] string? MessageId = null,
    [property: JsonPropertyName("error_message")] string? ErrorMessage = null,
    [property: JsonPropertyName("timestamp")] DateTime Timestamp = default
)
{
    public static SendAuthResponseDto FromAppModel(SendAuthResultAppModel model)
    {
        return new SendAuthResponseDto(
            Success: model.Success,
            MessageId: model.MessageId,
            ErrorMessage: model.ErrorMessage,
            Timestamp: model.Timestamp != default ? model.Timestamp : DateTime.UtcNow
        );
    }
}

