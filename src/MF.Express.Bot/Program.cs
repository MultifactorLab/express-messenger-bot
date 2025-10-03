using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using MF.Express.Bot.Api.Extensions;
using MF.Express.Bot.Api.Middleware;
using MF.Express.Bot.Infrastructure.Configuration;
using MF.Express.Bot.Infrastructure.Extensions;
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
    });
});

builder.AddLocalhostUserSecrets<Program>();

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

builder.Services.AddInfrastructure();
builder.Services.AddApplication();
builder.Services.AddExceptionHandler<ExpressBotExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddExpressBotHealthChecks();

var app = builder.Build();

app.UseExceptionHandler();
app.UseSerilogRequestLogging();

app.UseRouting();

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Local")
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MF Express Bot API v4");
        c.RoutePrefix = "swagger";
    });
}

app.UseBotXJwtValidation();

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapHealthChecks("/healthz");

app.MapSimpleEndpoints();

app.Run();