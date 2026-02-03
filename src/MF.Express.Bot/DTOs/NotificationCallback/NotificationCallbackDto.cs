using System.Text.Json.Serialization;
using MF.Express.Bot.Application.UseCases.Notifications;

namespace MF.Express.Bot.Api.DTOs.NotificationCallback;

public record NotificationCallbackDto(
    [property: JsonPropertyName("sync_id")] string SyncId,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("result")] object? Result = null,
    [property: JsonPropertyName("reason")] string? Reason = null,
    [property: JsonPropertyName("errors")] string[]? Errors = null,
    [property: JsonPropertyName("error_data")] object? ErrorData = null
)
{
    public static NotificationCallbackRequest ToRequest(NotificationCallbackDto dto)
    {
        return new NotificationCallbackRequest(
            SyncId: dto.SyncId,
            Status: dto.Status,
            Result: dto.Result,
            Reason: dto.Reason,
            Errors: dto.Errors,
            ErrorData: dto.ErrorData
        );
    }
}

