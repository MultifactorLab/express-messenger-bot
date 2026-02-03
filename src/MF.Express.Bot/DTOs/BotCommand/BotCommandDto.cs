using System.Text.Json.Serialization;
using MF.Express.Bot.Application.UseCases.BotCommands;

namespace MF.Express.Bot.Api.DTOs.BotCommand;

public record BotCommandDto(
    [property: JsonPropertyName("sync_id")] string SyncId,
    [property: JsonPropertyName("command")] BotCommandBodyDto Command,
    [property: JsonPropertyName("from")] BotCommandFromDto From,
    [property: JsonPropertyName("bot_id")] string BotId
)
{
    public static BotCommandRequest ToRequest(BotCommandDto dto)
    {
        return new BotCommandRequest(
            SyncId: dto.SyncId,
            CommandType: dto.Command.CommandType,
            CommandBody: dto.Command.Body,
            CommandData: dto.Command.Data,
            UserHuid: dto.From.UserHuid,
            GroupChatId: dto.From.GroupChatId,
            Username: dto.From.Username,
            Device: dto.From.Device,
            Locale: dto.From.Locale,
            BotId: dto.BotId
        );
    }
}

