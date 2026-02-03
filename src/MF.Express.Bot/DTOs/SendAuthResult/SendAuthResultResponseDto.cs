using System.Text.Json.Serialization;
using MF.Express.Bot.Application.UseCases.Auth;

namespace MF.Express.Bot.Api.DTOs.SendAuthResult;

public record SendAuthResultResponseDto(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("error_message")] string? ErrorMessage = null,
    [property: JsonPropertyName("error_type")] string? ErrorType = null,
    [property: JsonPropertyName("timestamp")] DateTime Timestamp = default
)
{
    public static SendAuthResultResponseDto FromResult(SendAuthResultResult result)
    {
        return new SendAuthResultResponseDto(
            Success: result.Success,
            ErrorMessage: result.ErrorMessage,
            ErrorType: result.ErrorType?.ToString(),
            Timestamp: result.Timestamp ?? DateTime.UtcNow
        );
    }
}


