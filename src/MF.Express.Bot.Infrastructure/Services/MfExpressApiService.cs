using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MF.Express.Bot.Application.Interfaces;
using MF.Express.Bot.Application.UseCases.BotCommands;
using MF.Express.Bot.Infrastructure.Configuration;

namespace MF.Express.Bot.Infrastructure.Services;

public class MfExpressApiService : IMfExpressApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly MfExpressApiConfiguration _configuration;
    private readonly ILogger<MfExpressApiService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public MfExpressApiService(
        IHttpClientFactory httpClientFactory,
        IOptions<MfExpressApiConfiguration> configuration,
        ILogger<MfExpressApiService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration.Value;
        _logger = logger;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<bool> SendAuthCallbackAsync(
        string callbackData,
        string chatId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending auth callback to MF Express API. ChatId: {ChatId:l}", chatId);

            var httpClient = _httpClientFactory.CreateClient("MfExpressApi");

            var payload = new
            {
                CallbackData = callbackData,
                ChatId = chatId
            };

            var json = JsonSerializer.Serialize(payload, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(_configuration.AuthCallbackEndpoint, content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Auth callback sent successfully to MF Express API");
                return true;
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Failed to send auth callback to MF Express API. Status: {Status:l}, Content: {Content:l}",
                response.StatusCode, responseContent);

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception sending auth callback to MF Express API");
            return false;
        }
    }

    public async Task<bool> SendChatCreatedCallbackAsync(BotCommandRequest botRequest, string authRequestId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending /start command to MF Express API. ChatId: {ChatId:l}, ExpressUserId: {ExpressUserId:l}, RequestId: {RequestId:l}",
                botRequest.GroupChatId, botRequest.UserHuid, authRequestId);

            var httpClient = _httpClientFactory.CreateClient("MfExpressApi");

            var payload = new
            {
                BotId = botRequest.BotId,
                ChatId = botRequest.GroupChatId,
                RequestId = authRequestId,
                ExpressUserId = botRequest.UserHuid,
                Username = botRequest.Username,
                Device = botRequest.Device,
                LanguageCode = botRequest.Locale
            };

            var json = JsonSerializer.Serialize(payload, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(_configuration.ChatCreatedEndpoint, content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("/start command sent successfully to MF Express API. ChatId: {ChatId:l}", botRequest.GroupChatId);
                return true;
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Failed to send /start command to MF Express API. Status: {Status:l}, Content: {Content:l}",
                response.StatusCode, responseContent);

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception sending /start command to MF Express API");
            return false;
        }
    }
}

