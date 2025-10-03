using System.Text.Json.Serialization;
using MF.Express.Bot.Application.Commands;

namespace MF.Express.Bot.Api.DTOs.BotCommand;

public record BotCommandDto(
    [property: JsonPropertyName("sync_id")] string SyncId,
    [property: JsonPropertyName("source_sync_id")] string? SourceSyncId,
    [property: JsonPropertyName("command")] BotCommandBodyDto Command,
    [property: JsonPropertyName("from")] BotCommandFromDto From,
    [property: JsonPropertyName("bot_id")] string BotId,
    [property: JsonPropertyName("proto_version")] int ProtoVersion,
    [property: JsonPropertyName("attachments")] List<object>? Attachments = null,
    [property: JsonPropertyName("async_files")] List<object>? AsyncFiles = null,
    [property: JsonPropertyName("entities")] List<object>? Entities = null
)
{
    public static ProcessBotXCommandCommand ToCommand(BotCommandDto dto)
    {
        return new ProcessBotXCommandCommand(
            SyncId: dto.SyncId,
            SourceSyncId: dto.SourceSyncId,
            CommandType: dto.Command.CommandType,
            CommandBody: dto.Command.Body,
            CommandData: dto.Command.Data,
            CommandMetadata: dto.Command.Metadata,
            UserHuid: dto.From.UserHuid,
            GroupChatId: dto.From.GroupChatId,
            ChatType: dto.From.ChatType,
            Username: dto.From.Username,
            AdLogin: dto.From.AdLogin,
            AdDomain: dto.From.AdDomain,
            IsAdmin: dto.From.IsAdmin,
            IsCreator: dto.From.IsCreator,
            Device: dto.From.Device,
            DeviceSoftware: dto.From.DeviceSoftware,
            Platform: dto.From.Platform,
            AppVersion: dto.From.AppVersion,
            Locale: dto.From.Locale,
            Host: dto.From.Host,
            BotId: dto.BotId,
            ProtoVersion: dto.ProtoVersion
        );
    }
}

