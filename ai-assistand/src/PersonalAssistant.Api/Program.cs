using Hangfire;
using PersonalAssistant.Api.Hubs;
using PersonalAssistant.Api.Middleware;
using PersonalAssistant.Application;
using PersonalAssistant.Application.Abstractions;
using PersonalAssistant.Infrastructure;
using PersonalAssistant.Infrastructure.HangfireJobs;
using PersonalAssistant.Infrastructure.Persistence;
using Serilog;

PersonalAssistant.Infrastructure.DependencyInjection.ConfigureSerilog();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<INotificationPublisher, SignalRNotificationPublisher>();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("web", policy =>
        policy.WithOrigins(
                "http://localhost:5173",
                "http://127.0.0.1:5173",
                "http://localhost:8080",
                "http://127.0.0.1:8080",
                "http://localhost",
                "http://127.0.0.1")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

var connectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? "Host=localhost;Port=5432;Database=personal_assistant;Username=postgres;Password=123456";

builder.Services.AddHealthChecks().AddNpgSql(connectionString);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSerilogRequestLogging();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("web");
app.MapControllers();
app.MapHub<AssistantHub>(AssistantHub.HubPath);
app.MapHealthChecks("/health");
app.UseHangfireDashboard("/hangfire");

RecurringJob.AddOrUpdate<MorningCheckInJob>("morning-check-in", job => job.ExecuteAsync(CancellationToken.None), "0 8 * * *");
RecurringJob.AddOrUpdate<EveningReviewJob>("evening-review", job => job.ExecuteAsync(CancellationToken.None), "0 20 * * *");

app.Run();

public partial class Program;
