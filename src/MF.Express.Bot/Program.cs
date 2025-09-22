using FluentValidation;
using FluentValidation.AspNetCore;
using MF.Express.Bot.Api.Extensions;
using MF.Express.Bot.Api.Middleware;
using MF.Express.Bot.Infrastructure.Configuration;
using MF.Express.Bot.Infrastructure.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Настройка логирования
builder.UseLogging();
builder.Services.AddAuthentication();

// API Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "MF Express Bot API", 
        Version = "v1",
        Description = "Прокси-бот для мессенджера Express. Предоставляет API для регистрации пользователей и отправки запросов авторизации."
    });
});

// Валидация
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Конфигурация
builder.Services.AddOptions<ExpressBotConfiguration>()
    .BindConfiguration(ExpressBotConfiguration.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Infrastructure services
builder.Services.AddInfrastructure();

// Exception handling
builder.Services.AddExceptionHandler<ExpressBotExceptionHandler>();
builder.Services.AddProblemDetails();

// Health checks
builder.Services.AddMfHealthChecks();

var app = builder.Build();

// Pipeline
app.UseExceptionHandler();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MF Express Bot API v1");
        c.RoutePrefix = string.Empty; // Swagger UI на корневом пути
    });
}

// Health checks
app.MapHealthChecks("/healthz");

// Express Bot Endpoints - чистая регистрация без захламления Program.cs
// Можно выбрать один из подходов:

// 1. Простая регистрация всех endpoints
app.MapSimpleEndpoints();

// 2. Альтернативно - группировка endpoints по функциональности (раскомментируйте при необходимости)
// app.MapBotEndpoints();

app.Run();