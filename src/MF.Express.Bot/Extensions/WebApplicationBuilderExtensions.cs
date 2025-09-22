using Serilog;

namespace MF.Express.Bot.Api.Extensions;

/// <summary>
/// Расширения для WebApplicationBuilder
/// </summary>
public static class WebApplicationBuilderExtensions
{
    /// <summary>
    /// Настраивает логирование с Serilog
    /// </summary>
    public static WebApplicationBuilder UseLogging(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();

        builder.Configuration
            .AddJsonFile($"serilog.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();
            
        builder.Services.AddHttpContextAccessor();

        builder.Host.UseSerilog((context, loggerConfiguration) => loggerConfiguration
            .Enrich.FromLogContext()
            .ReadFrom.Configuration(context.Configuration)
            .WriteTo.Console());

        return builder;
    }

    /// <summary>
    /// Добавляет пользовательские секреты для localhost окружения
    /// </summary>
    public static void AddLocalhostUserSecrets<TAssembly>(this WebApplicationBuilder builder) 
        where TAssembly : class
    {
        if (builder.Environment.EnvironmentName == "localhost")
        {
            builder.Configuration.AddUserSecrets<TAssembly>();
        }
    }
}

