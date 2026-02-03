namespace MF.Express.Bot.Application.Models.BotX;

public record BotStatusResultModel(
    bool Enabled,
    string? StatusMessage,
    List<BotCommandInfoModel> Commands
);

