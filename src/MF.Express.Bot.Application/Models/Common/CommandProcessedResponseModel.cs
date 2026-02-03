namespace MF.Express.Bot.Application.Models.Common;

public record CommandProcessedResponse(
    bool Success,
    string? ErrorMessage = null,
    DateTime ProcessedAt = default
)
{
    public CommandProcessedResponse() : this(true)
    {
        ProcessedAt = DateTime.UtcNow;
    }
}

