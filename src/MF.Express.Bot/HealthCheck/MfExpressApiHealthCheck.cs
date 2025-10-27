using MF.Express.Bot.Infrastructure.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace MF.Express.Bot.Api.HealthCheck;

internal sealed class MfExpressApiHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<MfExpressApiConfiguration> _config;
    private readonly ILogger<MfExpressApiHealthCheck> _logger;

    public MfExpressApiHealthCheck(
        IHttpClientFactory httpClientFactory,
        IOptions<MfExpressApiConfiguration> config,
        ILogger<MfExpressApiHealthCheck> logger)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("MfExpressApi");
            
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(5));

            var response = await client.SendAsync(
                new HttpRequestMessage(HttpMethod.Head, "/"), 
                cts.Token);

            if ((int)response.StatusCode < 500)
            {
                return HealthCheckResult.Healthy($"MfExpress API is reachable (Status: {response.StatusCode})");
            }
            
            _logger.LogWarning("MfExpress API returned server error: {StatusCode}", response.StatusCode);
            return HealthCheckResult.Unhealthy($"MfExpress API returned server error: {response.StatusCode}");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Unhealthy("Health check was cancelled");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("MfExpress API health check timeout");
            return HealthCheckResult.Unhealthy("MfExpress API timeout");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "MfExpress API health check failed - connection error");
            return HealthCheckResult.Unhealthy("MfExpress API is not accessible", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MfExpress API health check failed unexpectedly");
            return HealthCheckResult.Unhealthy("MfExpress API health check failed", ex);
        }
    }
}

