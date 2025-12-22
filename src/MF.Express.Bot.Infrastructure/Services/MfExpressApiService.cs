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
        _logger.LogInformation("Sending auth callback to MF Express API. ChatId: {ChatId:l}", chatId);

        var payload = new
        {
            CallbackData = callbackData,
            ChatId = chatId
        };

        return await SendPostRequestAsync(
            _configuration.AuthCallbackEndpoint,
            payload,
            "Auth callback",
            cancellationToken);
    }

    public async Task<bool> SendChatCreatedCallbackAsync(BotCommandRequest botRequest, string authRequestId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending /start command to MF Express API. ChatId: {ChatId:l}, ExpressUserId: {ExpressUserId:l}, RequestId: {RequestId:l}",
            botRequest.GroupChatId, botRequest.UserHuid, authRequestId);

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

        return await SendPostRequestAsync(
            _configuration.ChatCreatedEndpoint,
            payload,
            "/start command",
            cancellationToken);
    }

    private async Task<bool> SendPostRequestAsync(
        string endpoint,
        object payload,
        string operationName,
        CancellationToken cancellationToken)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("MfExpressApi");

            var json = JsonSerializer.Serialize(payload, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(endpoint, content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("{Operation} sent successfully to MF Express API", operationName);
                return true;
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Failed to send {Operation} to MF Express API. Status: {Status:l}, Response: {Response:l}",
                operationName, response.StatusCode, responseContent);

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception sending {Operation} to MF Express API", operationName);
            return false;
        }
    }
}

