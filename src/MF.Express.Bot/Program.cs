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

// Настройка логирования
builder.UseLogging();

// API Documentation
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

// JWT для Bot API v4
builder.Services.AddAuthentication();

// Валидация
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Конфигурация
builder.Services.AddOptions<ExpressBotConfiguration>()
    .BindConfiguration(ExpressBotConfiguration.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<MultifactorApiConfiguration>()
    .BindConfiguration(MultifactorApiConfiguration.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

// HTTP Clients для BotX API
builder.Services.AddHttpClient("BotX", (serviceProvider, client) =>
{
    var config = serviceProvider.GetRequiredService<IOptions<ExpressBotConfiguration>>().Value;
    client.BaseAddress = new Uri(config.BotXApiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(config.RequestTimeoutSeconds);
});

// Infrastructure services
builder.Services.AddInfrastructure();

// BotX API Service
builder.Services.AddScoped<IBotXApiService, BotXApiService>();

// Exception handling
builder.Services.AddExceptionHandler<ExpressBotExceptionHandler>();
builder.Services.AddProblemDetails();

// Health checks
builder.Services.AddMfHealthChecks();

var app = builder.Build();

// Pipeline
app.UseExceptionHandler();
app.UseSerilogRequestLogging();

// JWT валидация для Bot API v4 endpoints
app.UseBotXJwtValidation();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MF Express Bot API v4");
        c.RoutePrefix = string.Empty; // Swagger UI на корневом пути
    });
}

// Health checks
app.MapHealthChecks("/healthz");

// Bot API v4 Endpoints
app.MapSimpleEndpoints();

app.Run();