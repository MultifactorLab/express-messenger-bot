using System.Text.Json.Serialization;

namespace MF.Express.Bot.Application.DTOs;

/// <summary>
/// DTO для команд Bot API v4 от BotX
/// Основан на документации https://docs.express.ms/chatbots/developer-guide/api/bot-api/command/
/// </summary>
public record BotXCommandDto(
    [property: JsonPropertyName("sync_id")] string SyncId,
    [property: JsonPropertyName("source_sync_id")] string? SourceSyncId,
    [property: JsonPropertyName("command")] BotXCommandBodyDto Command,
    [property: JsonPropertyName("from")] BotXFromDto From,
    [property: JsonPropertyName("bot_id")] string BotId,
    [property: JsonPropertyName("proto_version")] int ProtoVersion,
    [property: JsonPropertyName("attachments")] List<object>? Attachments = null,
    [property: JsonPropertyName("async_files")] List<object>? AsyncFiles = null,
    [property: JsonPropertyName("entities")] List<object>? Entities = null
);

/// <summary>
/// Тело команды от BotX
/// </summary>
public record BotXCommandBodyDto(
    [property: JsonPropertyName("body")] string Body,
    [property: JsonPropertyName("command_type")] string CommandType,
    [property: JsonPropertyName("data")] Dictionary<string, object>? Data = null,
    [property: JsonPropertyName("metadata")] Dictionary<string, object>? Metadata = null
);

/// <summary>
/// Информация об отправителе команды
/// </summary>
public record BotXFromDto(
    [property: JsonPropertyName("user_huid")] string? UserHuid,
    [property: JsonPropertyName("group_chat_id")] string? GroupChatId,
    [property: JsonPropertyName("chat_type")] string? ChatType,
    [property: JsonPropertyName("ad_login")] string? AdLogin,
    [property: JsonPropertyName("ad_domain")] string? AdDomain,
    [property: JsonPropertyName("username")] string? Username,
    [property: JsonPropertyName("is_admin")] bool? IsAdmin,
    [property: JsonPropertyName("is_creator")] bool? IsCreator,
    [property: JsonPropertyName("manufacturer")] string? Manufacturer,
    [property: JsonPropertyName("device")] string? Device,
    [property: JsonPropertyName("device_software")] string? DeviceSoftware,
    [property: JsonPropertyName("device_meta")] Dictionary<string, object>? DeviceMeta,
    [property: JsonPropertyName("platform")] string? Platform,
    [property: JsonPropertyName("platform_package_id")] string? PlatformPackageId,
    [property: JsonPropertyName("app_version")] string? AppVersion,
    [property: JsonPropertyName("locale")] string? Locale,
    [property: JsonPropertyName("host")] string Host
);

/// <summary>
/// Стандартный ответ для Bot API v4 - 202 Accepted
/// </summary>
public record BotApiResponse(
    [property: JsonPropertyName("result")] string Result = "accepted"
);

/// <summary>
/// Ответ с ошибкой для Bot API v4 - 503 Bot Disabled
/// </summary>
public record BotApiErrorResponse(
    [property: JsonPropertyName("reason")] string Reason,
    [property: JsonPropertyName("error_data")] Dictionary<string, object> ErrorData,
    [property: JsonPropertyName("errors")] List<object> Errors
);
