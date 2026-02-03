using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MF.Express.Bot.Api.HealthCheck;

public static class WebApplicationBuilderExtensions
{
    public static void AddSelfHealthCheck(this WebApplicationBuilder builder)
    {
        if (builder.Environment.EnvironmentName == "test")
        {
            return;
        }
        
        builder.Services.AddHealthChecks().AddCheck(
            ApplicationHealthChecks.Self, 
            () => HealthCheckResult.Healthy("Application is running"),
            tags: ["live"]);
    }
    
    public static void AddBotXApiHealthCheck(this WebApplicationBuilder builder)
    {
        if (builder.Environment.EnvironmentName == "test")
        {
            return;
        }
        
        builder.Services.AddTransient<BotXApiHealthCheck>();
        
        var registration = new HealthCheckRegistration(
            name: ApplicationHealthChecks.BotXApi, 
            factory: prov => prov.GetRequiredService<BotXApiHealthCheck>(), 
            failureStatus: HealthStatus.Unhealthy, 
            tags: ["ready", "startup"],
            timeout: TimeSpan.FromSeconds(5));
        
        builder.Services.AddHealthChecks().Add(registration);
    }
    
    public static void AddMfExpressApiHealthCheck(this WebApplicationBuilder builder)
    {
        if (builder.Environment.EnvironmentName == "test")
        {
            return;
        }
        
        builder.Services.AddTransient<MfExpressApiHealthCheck>();
        
        var registration = new HealthCheckRegistration(
            name: ApplicationHealthChecks.MultifactorApi, 
            factory: prov => prov.GetRequiredService<MfExpressApiHealthCheck>(), 
            failureStatus: HealthStatus.Unhealthy, 
            tags: ["ready", "startup"],
            timeout: TimeSpan.FromSeconds(5));
        
        builder.Services.AddHealthChecks().Add(registration);
    }
}

