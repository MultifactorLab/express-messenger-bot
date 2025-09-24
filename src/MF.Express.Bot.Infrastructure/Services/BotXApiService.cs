using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MF.Express.Bot.Infrastructure.Configuration;
using MF.Express.Bot.Application.Interfaces;

namespace MF.Express.Bot.Infrastructure.Services;

/// <summary>
/// Сервис для взаимодействия с BotX API
/// Основан на документации https://docs.express.ms/chatbots/developer-guide/api/botx-api/
/// </summary>
public class BotXApiService : IBotXApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ExpressBotConfiguration _config;
    private readonly ILogger<BotXApiService> _logger;
    private readonly SemaphoreSlim _tokenSemaphore = new(1, 1);
    private string? _cachedToken;
    private DateTime _tokenExpiresAt = DateTime.MinValue;

    public BotXApiService(
        IHttpClientFactory httpClientFactory,
        IOptions<ExpressBotConfiguration> config,
        ILogger<BotXApiService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _config = config.Value;
        _logger = logger;
    }

    /// <summary>
    /// Отправляет текстовое сообщение в чат
    /// </summary>
    public async Task<bool> SendTextMessageAsync(string chatId, string text, CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await GetBotTokenAsync(cancellationToken);
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogError("Не удалось получить токен для отправки сообщения");
                return false;
            }

            var client = _httpClientFactory.CreateClient("BotX");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new
            {
                group_chat_id = chatId,
                notification = new
                {
                    status = "ok",
                    body = text
                }
            };

            var response = await client.PostAsJsonAsync("/api/v4/botx/notifications/direct", request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Сообщение успешно отправлено в чат {ChatId}", chatId);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Ошибка отправки сообщения в чат {ChatId}: {StatusCode} - {Content}", 
                chatId, response.StatusCode, errorContent);
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Исключение при отправке сообщения в чат {ChatId}", chatId);
            return false;
        }
    }

    /// <summary>
    /// Отправляет сообщение с inline кнопками
    /// </summary>
    public async Task<bool> SendMessageWithInlineKeyboardAsync(string chatId, string text, List<List<InlineKeyboardButton>> keyboard, CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await GetBotTokenAsync(cancellationToken);
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogError("Не удалось получить токен для отправки сообщения с кнопками");
                return false;
            }

            var client = _httpClientFactory.CreateClient("BotX");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new
            {
                group_chat_id = chatId,
                notification = new
                {
                    status = "ok",
                    body = text,
                    keyboard = keyboard.Select(row => row.Select(btn => new
                    {
                        command = btn.Data,
                        label = btn.Text
                    }))
                }
            };

            var response = await client.PostAsJsonAsync("/api/v4/botx/notifications/direct", request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Сообщение с кнопками успешно отправлено в чат {ChatId}", chatId);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Ошибка отправки сообщения с кнопками в чат {ChatId}: {StatusCode} - {Content}", 
                chatId, response.StatusCode, errorContent);
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Исключение при отправке сообщения с кнопками в чат {ChatId}", chatId);
            return false;
        }
    }

    /// <summary>
    /// Получает токен бота для BotX API
    /// Согласно документации: https://docs.express.ms/chatbots/developer-guide/api/botx-api/bots-api/
    /// </summary>
    private async Task<string?> GetBotTokenAsync(CancellationToken cancellationToken)
    {
        // Проверяем, есть ли действующий токен
        if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiresAt)
        {
            return _cachedToken;
        }

        await _tokenSemaphore.WaitAsync(cancellationToken);
        try
        {
            // Повторная проверка после получения семафора
            if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiresAt)
            {
                return _cachedToken;
            }

            _logger.LogDebug("Получение нового токена для бота {BotId}", _config.BotId);

            var client = _httpClientFactory.CreateClient("BotX");
            var signature = GenerateSignature(_config.BotId, _config.BotSecretKey);
            
            var tokenUrl = $"/api/v2/botx/bots/{_config.BotId}/token?signature={signature}";
            var response = await client.GetAsync(tokenUrl, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Ошибка получения токена: {StatusCode} - {Content}", response.StatusCode, errorContent);
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            using var jsonDoc = JsonDocument.Parse(responseContent);
            if (jsonDoc.RootElement.TryGetProperty("result", out var resultElement))
            {
                _cachedToken = resultElement.GetString();
                _tokenExpiresAt = DateTime.UtcNow.AddMinutes(55); // Токены обычно живут 1 час, берем с запасом
                
                _logger.LogInformation("Токен для бота успешно получен, действует до {ExpiresAt}", _tokenExpiresAt);
                return _cachedToken;
            }

            _logger.LogError("Некорректный формат ответа при получении токена: {Content}", responseContent);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Исключение при получении токена для бота {BotId}", _config.BotId);
            return null;
        }
        finally
        {
            _tokenSemaphore.Release();
        }
    }

    /// <summary>
    /// Генерирует подпись для получения токена
    /// </summary>
    private static string GenerateSignature(string botId, string secretKey)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var botIdBytes = Encoding.UTF8.GetBytes(botId);
        
        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(botIdBytes);
        
        return Convert.ToHexString(hashBytes);
    }

    /// <summary>
    /// Ответить на команду (reply)
    /// </summary>
    public async Task<bool> ReplyToCommandAsync(string syncId, string text, CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await GetBotTokenAsync(cancellationToken);
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Не удалось получить токен для ответа на команду {SyncId}", syncId);
                return false;
            }

            var client = _httpClientFactory.CreateClient("BotX");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new
            {
                result = new
                {
                    sync_id = syncId,
                    body = text
                }
            };

            var response = await client.PostAsJsonAsync("/api/v4/botx/command/result", request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Ответ на команду {SyncId} успешно отправлен", syncId);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Ошибка отправки ответа на команду {SyncId}: {StatusCode} - {Content}", 
                syncId, response.StatusCode, errorContent);
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Исключение при отправке ответа на команду {SyncId}", syncId);
            return false;
        }
    }

    /// <summary>
    /// Получить информацию о пользователе
    /// </summary>
    public async Task<BotXUserInfo?> GetUserInfoAsync(string userHuid, CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await GetBotTokenAsync(cancellationToken);
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Не удалось получить токен для получения информации о пользователе {UserHuid}", userHuid);
                return null;
            }

            var client = _httpClientFactory.CreateClient("BotX");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"/api/v3/botx/users/{userHuid}", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var userInfo = JsonSerializer.Deserialize<BotXUserInfo>(jsonContent);
                
                _logger.LogDebug("Информация о пользователе {UserHuid} получена", userHuid);
                return userInfo;
            }

            _logger.LogWarning("Ошибка получения информации о пользователе {UserHuid}: {StatusCode}", 
                userHuid, response.StatusCode);
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Исключение при получении информации о пользователе {UserHuid}", userHuid);
            return null;
        }
    }
}

