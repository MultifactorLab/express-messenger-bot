namespace MF.Express.Bot.Application.Models.NotificationCallback;

public record NotificationResultAppModel(
    bool Success,
    string? ErrorMessage = null,
    NotificationStatus? Status = null
);

