using System.Text.Json.Serialization;
using MF.Express.Bot.Application.Models.BotX;

namespace MF.Express.Bot.Api.DTOs.BotStatus;

public record BotCommandInfoDto(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("body")] string Body,
    [property: JsonPropertyName("description")] string Description
)
{
    public static BotCommandInfoDto FromAppModel(BotCommandInfoModel model)
    {
        return new BotCommandInfoDto(
            Name: model.Name,
            Body: model.Body,
            Description: model.Description
        );
    }
}

