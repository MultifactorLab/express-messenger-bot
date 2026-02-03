namespace MF.Express.Bot.Application.Models.SendAuthRequest;

public record SendAuthResultAppModel(
    bool Success,
    string? MessageId = null,
    string? ErrorMessage = null,
    DateTime Timestamp = default
)
{
    public SendAuthResultAppModel() : this(false)
    {
        Timestamp = DateTime.UtcNow;
    }
}

