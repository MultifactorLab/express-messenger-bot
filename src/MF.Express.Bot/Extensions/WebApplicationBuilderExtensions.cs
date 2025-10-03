using Serilog;

namespace MF.Express.Bot.Api.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder UseLogging(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();

        builder.Configuration
            .AddJsonFile($"serilog.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();
            
        builder.Services.AddHttpContextAccessor();

        builder.Host.UseSerilog((context, loggerConfiguration) => loggerConfiguration
            .ReadFrom.Configuration(context.Configuration));

        return builder;
    }
    public static void AddLocalhostUserSecrets<TAssembly>(this WebApplicationBuilder builder) 
        where TAssembly : class
    {
        if (builder.Environment.EnvironmentName == "localhost")
        {
            builder.Configuration.AddUserSecrets<TAssembly>();
        }
    }
}