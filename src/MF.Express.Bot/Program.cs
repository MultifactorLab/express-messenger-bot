using FluentValidation;
using FluentValidation.AspNetCore;
using MF.Express.Bot.Api.Extensions;
using MF.Express.Bot.Api.Middleware;
using MF.Express.Bot.Application.Commands;
using MF.Express.Bot.Application.DTOs;
using MF.Express.Bot.Application.Interfaces;
using MF.Express.Bot.Infrastructure.Configuration;
using MF.Express.Bot.Infrastructure.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Настройка логирования
builder.UseLogging();
builder.Services.AddAuthentication();

// API Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
    app.UseSwaggerUI();
}

// Health checks
app.MapHealthChecks("/healthz");

// Express Bot Proxy API endpoints
app.MapPost("/register-user", async (
    RegisterUserRequestDto request,
    ICommand<RegisterUserCommand, RegisterUserResultDto> handler,
    CancellationToken ct) =>
{
    var command = new RegisterUserCommand(
        request.ChatId,
        request.UserId,
        request.Username,
        request.FirstName,
        request.LastName,
        request.Metadata);
        
    var result = await handler.Handle(command, ct);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
})
.WithName("RegisterUser")
.WithOpenApi()
.WithSummary("Регистрация пользователя в чате с ботом");

app.MapPost("/send-auth-request", async (
    SendAuthRequestDto request,
    ICommand<SendAuthRequestCommand, SendAuthResultDto> handler,
    CancellationToken ct) =>
{
    var command = new SendAuthRequestCommand(
        request.ChatId,
        request.UserId,
        request.AuthRequestId,
        request.Message,
        request.ResourceName,
        request.Metadata);
        
    var result = await handler.Handle(command, ct);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
})
.WithName("SendAuthRequest")
.WithOpenApi()
.WithSummary("Отправка запроса авторизации с кнопками подтверждения");

// Manual message sending (для тестирования/административных целей)
app.MapPost("/send", async (
    SendMessageRequestDto request,
    ICommand<SendMessageCommand, SendMessageResultDto> handler,
    CancellationToken ct) =>
{
    var command = new SendMessageCommand(
        request.ChatId, 
        request.Text, 
        request.Type, 
        request.ReplyToMessageId, 
        request.Metadata);
        
    var result = await handler.Handle(command, ct);
    return Results.Ok(result);
})
.WithName("SendMessage")
.WithOpenApi()
.WithSummary("Отправка сообщения через бота");

// Bot status endpoint
app.MapGet("/status", async (IExpressBotService botService, CancellationToken ct) =>
{
    var botInfo = await botService.GetBotInfoAsync(ct);
    return Results.Ok(new
    {
        BotInfo = botInfo,
        Status = "Running",
        Timestamp = DateTime.UtcNow
    });
})
.WithName("GetBotStatus")
.WithOpenApi()
.WithSummary("Получить статус бота");

app.Run();