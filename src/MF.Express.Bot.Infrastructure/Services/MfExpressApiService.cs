using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MF.Express.Bot.Application.Interfaces;
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
            _logger.LogInformation("Отправка auth callback в MF Express API. ChatId: {ChatId}", chatId);

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
                _logger.LogInformation("Auth callback успешно отправлен в MF Express API");
                return true;
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Ошибка при отправке auth callback в MF Express API. Status: {Status}, Content: {Content}",
                response.StatusCode, responseContent);

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Исключение при отправке auth callback в MF Express API");
            return false;
        }
    }
}

