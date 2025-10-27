using System.Text.Json.Serialization;
using MF.Express.Bot.Application.UseCases.Greeting;

namespace MF.Express.Bot.Api.DTOs.SendGreeting;

public record SendGreetingDto(
    [property: JsonPropertyName("chat_id")] string ChatId,
    [property: JsonPropertyName("message")] string Message
)
{
    public static SendGreetingRequest ToRequest(SendGreetingDto dto)
    {
        return new SendGreetingRequest(
            ChatId: dto.ChatId,
            Message: dto.Message
        );
    }
}

