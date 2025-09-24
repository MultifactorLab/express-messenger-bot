using System.Text;
using System.Text.Json;
using MF.Express.Bot.Application.Commands;
using MF.Express.Bot.Application.DTOs;
using MF.Express.Bot.Application.Interfaces;
using MF.Express.Bot.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MF.Express.Bot.Infrastructure.ExternalServices;

/// <summary>
/// Реализация сервиса для работы с BotX API v4
/// Используется для отправки сообщений через Express.MS BotX
/// </summary>
public class ExpressBotService : IExpressBotService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ExpressBotConfiguration _config;
    private readonly ILogger<ExpressBotService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ExpressBotService(
        IHttpClientFactory httpClientFactory,
        IOptions<ExpressBotConfiguration> config,
        ILogger<ExpressBotService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _config = config.Value;
        _logger = logger;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = false
        };
    }

    public async Task<SendMessageResultDto> SendTextMessageAsync(string chatId, string text, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Отправка текстового сообщения в чат {ChatId}", chatId);

            var httpClient = _httpClientFactory.CreateClient("ExpressBot");
            var request = new
            {
                chat_id = chatId,
                text = text,
                parse_mode = "HTML"
            };

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("bot/sendMessage", content, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ExpressApiResponse>(responseContent, _jsonOptions);
                var messageId = result?.Result?.MessageId ?? Guid.NewGuid().ToString();

                _logger.LogDebug("Сообщение успешно отправлено. MessageId: {MessageId}", messageId);
                return new SendMessageResultDto(messageId, true, null, DateTime.UtcNow);
            }
            else
            {
                _logger.LogWarning("Не удалось отправить сообщение. Status: {StatusCode}, Response: {Response}", 
                    response.StatusCode, responseContent);
                return new SendMessageResultDto(string.Empty, false, $"HTTP {response.StatusCode}: {responseContent}", DateTime.UtcNow);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке текстового сообщения в чат {ChatId}", chatId);
            return new SendMessageResultDto(string.Empty, false, ex.Message, DateTime.UtcNow);
        }
    }

    public async Task<SendMessageResultDto> SendMessageWithButtonsAsync(string chatId, string text, InlineKeyboardMarkup inlineKeyboard, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Отправка сообщения с кнопками в чат {ChatId}", chatId);

            var httpClient = _httpClientFactory.CreateClient("ExpressBot");
            var request = new
            {
                chat_id = chatId,
                text = text,
                parse_mode = "HTML",
                reply_markup = new
                {
                    inline_keyboard = inlineKeyboard.Keyboard.Select(row => 
                        row.Select(button => new
                        {
                            text = button.Text,
                            callback_data = button.CallbackData
                        }).ToArray()
                    ).ToArray()
                }
            };

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("bot/sendMessage", content, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ExpressApiResponse>(responseContent, _jsonOptions);
                var messageId = result?.Result?.MessageId ?? Guid.NewGuid().ToString();

                _logger.LogDebug("Сообщение с кнопками успешно отправлено. MessageId: {MessageId}", messageId);
                return new SendMessageResultDto(messageId, true, null, DateTime.UtcNow);
            }
            else
            {
                _logger.LogWarning("Не удалось отправить сообщение с кнопками. Status: {StatusCode}, Response: {Response}", 
                    response.StatusCode, responseContent);
                return new SendMessageResultDto(string.Empty, false, $"HTTP {response.StatusCode}: {responseContent}", DateTime.UtcNow);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке сообщения с кнопками в чат {ChatId}", chatId);
            return new SendMessageResultDto(string.Empty, false, ex.Message, DateTime.UtcNow);
        }
    }

    public async Task<SendMessageResultDto> SendFileAsync(string chatId, Stream file, string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Отправка файла {FileName} в чат {ChatId}", fileName, chatId);

            var httpClient = _httpClientFactory.CreateClient("ExpressBot");
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(chatId), "chat_id");
            content.Add(new StreamContent(file), "document", fileName);

            var response = await httpClient.PostAsync("bot/sendDocument", content, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ExpressApiResponse>(responseContent, _jsonOptions);
                var messageId = result?.Result?.MessageId ?? Guid.NewGuid().ToString();

                _logger.LogDebug("Файл успешно отправлен. MessageId: {MessageId}", messageId);
                return new SendMessageResultDto(messageId, true, null, DateTime.UtcNow);
            }
            else
            {
                _logger.LogWarning("Не удалось отправить файл. Status: {StatusCode}, Response: {Response}", 
                    response.StatusCode, responseContent);
                return new SendMessageResultDto(string.Empty, false, $"HTTP {response.StatusCode}: {responseContent}", DateTime.UtcNow);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке файла {FileName} в чат {ChatId}", fileName, chatId);
            return new SendMessageResultDto(string.Empty, false, ex.Message, DateTime.UtcNow);
        }
    }

    public async Task<BotInfoDto> GetBotInfoAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Получение информации о боте");

            var httpClient = _httpClientFactory.CreateClient("ExpressBot");
            var response = await httpClient.GetAsync("bot/getMe", cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ExpressBotInfoResponse>(responseContent, _jsonOptions);
                var botInfo = result?.Result;

                if (botInfo != null)
                {
                    _logger.LogDebug("Информация о боте получена: {BotName} (@{BotUsername})", botInfo.Name, botInfo.Username);
                    return new BotInfoDto(botInfo.Id, botInfo.Name, botInfo.Username, true, DateTime.UtcNow);
                }
            }

            _logger.LogWarning("Не удалось получить информацию о боте. Status: {StatusCode}", response.StatusCode);
            return new BotInfoDto(string.Empty, "Unknown", "unknown", false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении информации о боте");
            return new BotInfoDto(string.Empty, "Error", "error", false);
        }
    }


    #region API Response Models

    private class ExpressApiResponse
    {
        public bool Ok { get; set; }
        public ExpressApiResult? Result { get; set; }
        public string? Description { get; set; }
    }

    private class ExpressApiResult
    {
        public string MessageId { get; set; } = string.Empty;
    }

    private class ExpressBotInfoResponse
    {
        public bool Ok { get; set; }
        public ExpressBotInfo? Result { get; set; }
    }

    private class ExpressBotInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
    }

    #endregion
}
