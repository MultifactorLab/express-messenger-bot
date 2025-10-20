using System.Text.Json.Serialization;
using MF.Express.Bot.Application.UseCases.BotCommands;

namespace MF.Express.Bot.Api.DTOs.BotCommand;

public record BotCommandDto(
    [property: JsonPropertyName("sync_id")] string SyncId,
    [property: JsonPropertyName("command")] BotCommandBodyDto Command,
    [property: JsonPropertyName("from")] BotCommandFromDto From,
    [property: JsonPropertyName("bot_id")] string BotId,
    [property: JsonPropertyName("proto_version")] int ProtoVersion
)
{
    public static BotCommandRequest ToRequest(BotCommandDto dto)
    {
        return new BotCommandRequest(
            SyncId: dto.SyncId,
            CommandType: dto.Command.CommandType,
            CommandBody: dto.Command.Body,
            CommandData: dto.Command.Data,
            CommandMetadata: dto.Command.Metadata,
            UserHuid: dto.From.UserHuid,
            GroupChatId: dto.From.GroupChatId,
            ChatType: dto.From.ChatType,
            Username: dto.From.Username,
            Device: dto.From.Device,
            Locale: dto.From.Locale,
            Host: dto.From.Host,
            BotId: dto.BotId,
            ProtoVersion: dto.ProtoVersion
        );
    }
}

