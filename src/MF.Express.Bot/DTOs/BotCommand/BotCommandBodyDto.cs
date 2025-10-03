using System.Text.Json.Serialization;

namespace MF.Express.Bot.Api.DTOs.BotCommand;

public record BotCommandBodyDto(
    [property: JsonPropertyName("body")] string Body,
    [property: JsonPropertyName("command_type")] string CommandType,
    [property: JsonPropertyName("data")] Dictionary<string, object>? Data = null,
    [property: JsonPropertyName("metadata")] Dictionary<string, object>? Metadata = null
);

