using System.Text.Json.Serialization;

namespace MF.Express.Bot.Api.DTOs.VerifyBot;

public record VerifyBotResponseDto(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("bot_id")] string BotId,
    [property: JsonPropertyName("verified")] bool Verified
);



