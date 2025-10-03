using System.Text.Json.Serialization;
using MF.Express.Bot.Application.Models.BotCommand;

namespace MF.Express.Bot.Api.DTOs.BotCommand;

public record BotApiResponseDto(
    [property: JsonPropertyName("result")] string Result = "accepted"
)
{
    public static BotApiResponseDto FromAppModel(BotApiResponseAppModel model)
    {
        return new BotApiResponseDto(model.Result);
    }
}

