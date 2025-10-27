using System.Text.Json.Serialization;
using MF.Express.Bot.Application.UseCases.Greeting;

namespace MF.Express.Bot.Api.DTOs.SendGreeting;

public record SendGreetingResponseDto(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("error_message")] string? ErrorMessage = null,
    [property: JsonPropertyName("error_type")] string? ErrorType = null,
    [property: JsonPropertyName("timestamp")] DateTime Timestamp = default
)
{
    public static SendGreetingResponseDto FromResult(SendGreetingResult result)
    {
        return new SendGreetingResponseDto(
            Success: result.Success,
            ErrorMessage: result.ErrorMessage,
            ErrorType: result.ErrorType?.ToString(),
            Timestamp: result.Timestamp ?? DateTime.UtcNow
        );
    }
}

