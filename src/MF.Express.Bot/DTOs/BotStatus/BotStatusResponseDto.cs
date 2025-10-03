using System.Text.Json.Serialization;
using MF.Express.Bot.Application.Models.BotX;

namespace MF.Express.Bot.Api.DTOs.BotStatus;

public record BotStatusResponseDto(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("result")] BotStatusResultDto? Result
)
{
    public static BotStatusResponseDto FromAppModel(BotStatusModel model)
    {
        return new BotStatusResponseDto(
            Status: model.Status,
            Result: model.Result != null ? BotStatusResultDto.FromAppModel(model.Result) : null
        );
    }
}

