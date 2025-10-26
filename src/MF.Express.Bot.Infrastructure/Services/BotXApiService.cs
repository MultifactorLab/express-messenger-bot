using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MF.Express.Bot.Application.Interfaces;
using MF.Express.Bot.Application.Models.BotX;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MF.Express.Bot.Infrastructure.Configuration;

namespace MF.Express.Bot.Infrastructure.Services;

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

    public async Task<bool> SendTextMessageAsync(string chatId, string text, CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await GetBotTokenAsync(cancellationToken);
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogError("Failed to get token for sending message");
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
                _logger.LogInformation("Message sent successfully. ChatId: {ChatId:l}", chatId);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Failed to send message. ChatId: {ChatId:l}, StatusCode: {StatusCode:l}, Content: {Content:l}", 
                chatId, response.StatusCode, errorContent);
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception sending message. ChatId: {ChatId:l}", chatId);
            return false;
        }
    }

    public async Task<bool> SendMessageWithInlineKeyboardAsync(string chatId, string text, List<List<InlineKeyboardButtonModel>> keyboard, CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await GetBotTokenAsync(cancellationToken);
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogError("Failed to get token for sending message with buttons");
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
                    bubble = keyboard.Select(row => row.Select(btn => new
                    {
                        command = btn.Data,
                        label = btn.Text,
                        opts = new { silent = true }
                    }))
                }
            };

            var response = await client.PostAsJsonAsync("/api/v4/botx/notifications/direct", request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Message with buttons sent successfully. ChatId: {ChatId:l}", chatId);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Failed to send message with buttons. ChatId: {ChatId:l}, StatusCode: {StatusCode:l}, Content: {Content:l}", 
                chatId, response.StatusCode, errorContent);
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception sending message with buttons. ChatId: {ChatId:l}", chatId);
            return false;
        }
    }

    private async Task<string?> GetBotTokenAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiresAt)
        {
            return _cachedToken;
        }

        await _tokenSemaphore.WaitAsync(cancellationToken);
        try
        {
            if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiresAt)
            {
                return _cachedToken;
            }

            _logger.LogDebug("Getting new token. BotId: {BotId:l}", _config.BotId);

            var client = _httpClientFactory.CreateClient("BotX");
            var signature = GenerateSignature(_config.BotId, _config.BotSecretKey);
            
            var tokenUrl = $"/api/v2/botx/bots/{_config.BotId}/token?signature={signature}";
            var response = await client.GetAsync(tokenUrl, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Token request failed. StatusCode: {StatusCode:l}, Content: {Content:l}", response.StatusCode, errorContent);
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            using var jsonDoc = JsonDocument.Parse(responseContent);
            if (jsonDoc.RootElement.TryGetProperty("result", out var resultElement))
            {
                _cachedToken = resultElement.GetString();
                _tokenExpiresAt = DateTime.UtcNow.AddMinutes(55);
                
                _logger.LogInformation("Token obtained successfully. ExpiresAt: {ExpiresAt:l}", _tokenExpiresAt);
                return _cachedToken;
            }

            _logger.LogError("Invalid token response format. Content: {Content:l}", responseContent);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception getting token. BotId: {BotId:l}", _config.BotId);
            return null;
        }
        finally
        {
            _tokenSemaphore.Release();
        }
    }

    private static string GenerateSignature(string botId, string secretKey)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var botIdBytes = Encoding.UTF8.GetBytes(botId);
        
        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(botIdBytes);
        
        return Convert.ToHexString(hashBytes);
    }

    public async Task<bool> ReplyToCommandAsync(string syncId, string text, CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await GetBotTokenAsync(cancellationToken);
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Failed to get token for command reply. SyncId: {SyncId:l}", syncId);
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
                _logger.LogInformation("Command reply sent successfully. SyncId: {SyncId:l}", syncId);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Failed to send command reply. SyncId: {SyncId:l}, StatusCode: {StatusCode:l}, Content: {Content:l}", 
                syncId, response.StatusCode, errorContent);
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception sending command reply. SyncId: {SyncId:l}", syncId);
            return false;
        }
    }

    public async Task<BotXUserInfoModel?> GetUserInfoAsync(string userHuid, CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await GetBotTokenAsync(cancellationToken);
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Failed to get token for user info. UserHuid: {UserHuid:l}", userHuid);
                return null;
            }

            var client = _httpClientFactory.CreateClient("BotX");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"/api/v3/botx/users/{userHuid}", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var userInfo = JsonSerializer.Deserialize<BotXUserInfoModel>(jsonContent);
                
                _logger.LogDebug("User info retrieved. UserHuid: {UserHuid:l}", userHuid);
                return userInfo;
            }

            _logger.LogWarning("Failed to get user info. UserHuid: {UserHuid:l}, StatusCode: {StatusCode:l}", 
                userHuid, response.StatusCode);
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception getting user info. UserHuid: {UserHuid:l}", userHuid);
            return null;
        }
    }
}

