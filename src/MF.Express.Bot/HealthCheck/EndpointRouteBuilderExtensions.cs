using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MF.Express.Bot.Api.HealthCheck;

public static class EndpointRouteBuilderExtensions
{
    public static void UseHealthCheck(this IEndpointRouteBuilder app)
    {
        var env = app.ServiceProvider.GetRequiredService<IHostEnvironment>();
        if (env.EnvironmentName == "test")
        {
            return;
        }
        
        app.MapHealthChecks("/health/startup", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("startup"),
            ResponseWriter = MinimalWriter
        });
        
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("ready"),
            ResponseWriter = MinimalWriter
        });
        
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("live"),
            ResponseWriter = MinimalWriter
        });
    }
    
    private static Task MinimalWriter(HttpContext ctx, HealthReport report)
    {
        ctx.Response.ContentType = "text/plain";
        ctx.Response.StatusCode = report.Status == HealthStatus.Healthy ? 200 : 500;
        return ctx.Response.WriteAsync(report.Status.ToString());
    }
}

