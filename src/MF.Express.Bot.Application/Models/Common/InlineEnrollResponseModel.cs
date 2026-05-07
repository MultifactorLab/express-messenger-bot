namespace MF.Express.Bot.Application.Models.Common;

public record InlineEnrollResponse(
    bool Success,
    string? Message = null,
    string? ErrorMessage = null
);
