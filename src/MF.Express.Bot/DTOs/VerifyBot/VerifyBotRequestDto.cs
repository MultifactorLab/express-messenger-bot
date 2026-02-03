using System.ComponentModel.DataAnnotations;

namespace MF.Express.Bot.Api.DTOs.VerifyBot;

public record VerifyBotRequestDto(
    [Required]
    string BotId,
    
    [Required]
    string Signature
);



