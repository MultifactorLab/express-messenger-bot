using System.Text.Json.Serialization;

namespace MF.Express.Bot.Api.DTOs.Common;
public record BotApiErrorResponseDto(
    [property: JsonPropertyName("reason")] string Reason,
    [property: JsonPropertyName("error_data")] Dictionary<string, object> ErrorData,
    [property: JsonPropertyName("errors")] List<object> Errors
);

