using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.Options;
using MF.Express.Bot.Api.Extensions;
using MF.Express.Bot.Api.Middleware;
using MF.Express.Bot.Application.Interfaces;
using MF.Express.Bot.Infrastructure.Configuration;
using MF.Express.Bot.Infrastructure.Extensions;
using MF.Express.Bot.Infrastructure.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.UseLogging();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "MF Express Bot API v4", 
        Version = "v1",
        Description = "Express.MS Bot API v4 для мессенджера Express. Поддерживает Bot API v4 и BotX API интеграцию для двухфакторной аутентификации."
    });
});

builder.Services.AddAuthentication();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddOptions<ExpressBotConfiguration>()
    .BindConfiguration(ExpressBotConfiguration.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<MultifactorApiConfiguration>()
    .BindConfiguration(MultifactorApiConfiguration.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddHttpClient("BotX", (serviceProvider, client) =>
{
    var config = serviceProvider.GetRequiredService<IOptions<ExpressBotConfiguration>>().Value;
    client.BaseAddress = new Uri(config.BotXApiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(config.RequestTimeoutSeconds);
});

builder.Services.AddInfrastructure();

builder.Services.AddApplication();


builder.Services.AddExceptionHandler<ExpressBotExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddMfHealthChecks();

var app = builder.Build();

app.UseExceptionHandler();
app.UseSerilogRequestLogging();

app.UseBotXJwtValidation();

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Local")
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MF Express Bot API v4");
        c.RoutePrefix = "swagger";
    });
}

app.MapHealthChecks("/healthz");

app.MapSimpleEndpoints();

app.Run();