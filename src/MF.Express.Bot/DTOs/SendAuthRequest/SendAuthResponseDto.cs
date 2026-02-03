using System.Text.Json.Serialization;
using MF.Express.Bot.Application.UseCases.Auth;

namespace MF.Express.Bot.Api.DTOs.SendAuthRequest;

public record SendAuthResponseDto(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("message_id")] string? MessageId = null,
    [property: JsonPropertyName("error_message")] string? ErrorMessage = null,
    [property: JsonPropertyName("error_type")] string? ErrorType = null,
    [property: JsonPropertyName("timestamp")] DateTime Timestamp = default
)
{
    public static SendAuthResponseDto FromResult(SendAuthRequestResult result)
    {
        return new SendAuthResponseDto(
            Success: result.Success,
            MessageId: null,
            ErrorMessage: result.ErrorMessage,
            ErrorType: result.ErrorType?.ToString(),
            Timestamp: result.Timestamp ?? DateTime.UtcNow
        );
    }
}

