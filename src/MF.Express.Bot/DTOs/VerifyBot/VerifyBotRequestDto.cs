using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MF.Express.Bot.Api.DTOs.VerifyBot;

public record VerifyBotRequestDto(
    [property: JsonPropertyName("bot_id")]
    [Required]
    string BotId,
    
    [property: JsonPropertyName("signature")]
    [Required]
    string Signature
);



