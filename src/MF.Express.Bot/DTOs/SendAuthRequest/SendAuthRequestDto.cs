using System.Text.Json.Serialization;
using MF.Express.Bot.Application.UseCases.Auth;

namespace MF.Express.Bot.Api.DTOs.SendAuthRequest;

public record SendAuthRequestDto(
    [property: JsonPropertyName("chat_id")] string ChatId,
    [property: JsonPropertyName("user_id")] string UserId,
    [property: JsonPropertyName("auth_request_id")] string AuthRequestId,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("approve_button_text")] string ApproveButtonText = null,
    [property: JsonPropertyName("reject_button_text")] string RejectButtonText = null,
    [property: JsonPropertyName("approve_button_callback_data")] string ApproveButtonCallbackData = null,
    [property: JsonPropertyName("reject_button_callback_data")] string RejectButtonCallbackData = null
)
{
    public static SendAuthRequestRequest ToRequest(SendAuthRequestDto dto)
    {
        return new SendAuthRequestRequest(
            ChatId: dto.ChatId,
            UserId: dto.UserId,
            AuthRequestId: dto.AuthRequestId,
            Message: dto.Message,
            ApproveButtonText: dto.ApproveButtonText,
            RejectButtonText: dto.RejectButtonText,
            ApproveButtonCallbackData: dto.ApproveButtonCallbackData,
            RejectButtonCallbackData: dto.RejectButtonCallbackData
        );
    }
}

