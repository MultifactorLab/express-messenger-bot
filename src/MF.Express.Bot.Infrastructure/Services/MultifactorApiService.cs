using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MF.Express.Bot.Application.DTOs;
using MF.Express.Bot.Application.Interfaces;
using MF.Express.Bot.Infrastructure.Configuration;

namespace MF.Express.Bot.Infrastructure.Services;

/// <summary>
/// Сервис для взаимодействия с Multifactor API
/// </summary>
public class MultifactorApiService : IMultifactorApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly MultifactorApiConfiguration _configuration;
    private readonly ILogger<MultifactorApiService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public MultifactorApiService(
        IHttpClientFactory httpClientFactory,
        IOptions<MultifactorApiConfiguration> configuration,
        ILogger<MultifactorApiService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration.Value;
        _logger = logger;

        // Настройка JSON сериализации
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<bool> SendUserChatInfoAsync(UserChatInfoDto userChatInfo, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Отправка информации о пользователе {UserId} и чате {ChatId} в Multifactor API", 
                userChatInfo.UserId, userChatInfo.ChatId);

            var httpClient = _httpClientFactory.CreateClient("MultifactorApi");
            var json = JsonSerializer.Serialize(userChatInfo, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(_configuration.UserChatInfoEndpoint, content, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Информация о пользователе {UserId} успешно отправлена в Multifactor API", userChatInfo.UserId);
                return true;
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Ошибка при отправке информации о пользователе в Multifactor API. Status: {Status}, Content: {Content}", 
                response.StatusCode, responseContent);
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Исключение при отправке информации о пользователе {UserId} в Multifactor API", userChatInfo.UserId);
            return false;
        }
    }

    public async Task<bool> SendAuthorizationResultAsync(AuthorizationResultDto authResult, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Отправка результата авторизации {AuthRequestId} для пользователя {UserId} в Multifactor API", 
                authResult.AuthRequestId, authResult.UserId);

            var httpClient = _httpClientFactory.CreateClient("MultifactorApi");
            var json = JsonSerializer.Serialize(authResult, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(_configuration.AuthorizationResultEndpoint, content, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Результат авторизации {AuthRequestId} успешно отправлен в Multifactor API", authResult.AuthRequestId);
                return true;
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Ошибка при отправке результата авторизации в Multifactor API. Status: {Status}, Content: {Content}", 
                response.StatusCode, responseContent);
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Исключение при отправке результата авторизации {AuthRequestId} в Multifactor API", authResult.AuthRequestId);
            return false;
        }
    }

    public async Task<UserInfoResponse?> GetUserInfoAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Запрос информации о пользователе {UserId} из Multifactor API", userId);

            var httpClient = _httpClientFactory.CreateClient("MultifactorApi");
            var response = await httpClient.GetAsync($"/api/users/{userId}", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                var userInfo = JsonSerializer.Deserialize<UserInfoResponse>(json, _jsonOptions);
                
                _logger.LogInformation("Информация о пользователе {UserId} получена из Multifactor API", userId);
                return userInfo;
            }

            _logger.LogWarning("Не удалось получить информацию о пользователе {UserId} из Multifactor API. Status: {Status}", 
                userId, response.StatusCode);
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Исключение при запросе информации о пользователе {UserId} из Multifactor API", userId);
            return null;
        }
    }
}
