using MF.Express.Bot.Application.Models.BotX;

namespace MF.Express.Bot.Application.Interfaces;

public interface IBotXApiService
{
    Task<bool> SendTextMessageAsync(
        string chatId,
        string text, 
        CancellationToken cancellationToken = default);

    Task<bool>  SendMessageWithInlineKeyboardAsync(
        string chatId,
        string text,
        List<List<InlineKeyboardButtonModel>> keyboard,
        CancellationToken cancellationToken = default);
}
