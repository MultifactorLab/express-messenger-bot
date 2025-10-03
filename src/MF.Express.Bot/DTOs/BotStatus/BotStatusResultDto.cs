using System.Text.Json.Serialization;
using MF.Express.Bot.Application.Models.BotX;

namespace MF.Express.Bot.Api.DTOs.BotStatus;

public record BotStatusResultDto(
    [property: JsonPropertyName("enabled")] bool Enabled,
    [property: JsonPropertyName("status_message")] string? StatusMessage,
    [property: JsonPropertyName("commands")] List<BotCommandInfoDto> Commands
)
{
    public static BotStatusResultDto FromAppModel(BotStatusResultModel model)
    {
        return new BotStatusResultDto(
            Enabled: model.Enabled,
            StatusMessage: model.StatusMessage,
            Commands: model.Commands.Select(BotCommandInfoDto.FromAppModel).ToList()
        );
    }
}

