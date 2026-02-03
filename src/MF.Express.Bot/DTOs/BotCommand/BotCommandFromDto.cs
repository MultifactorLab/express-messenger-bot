using System.Text.Json.Serialization;

namespace MF.Express.Bot.Api.DTOs.BotCommand;

public record BotCommandFromDto(
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

