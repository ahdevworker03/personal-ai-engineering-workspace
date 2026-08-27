using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PersonalAssistant.Application.Abstractions;
using PersonalAssistant.Infrastructure.DeepSeek;
using PersonalAssistant.Infrastructure.FishAudio;
using PersonalAssistant.Infrastructure.HangfireJobs;
using PersonalAssistant.Infrastructure.Persistence;
using Serilog;

namespace PersonalAssistant.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? "Host=localhost;Port=5432;Database=personal_assistant;Username=postgres;Password=123456";

        services.Configure<DeepSeekOptions>(configuration.GetSection("DeepSeek"));
        services.Configure<FishAudioOptions>(configuration.GetSection("FishAudio"));

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddHttpClient<IChatModelService, DeepSeekChatModelService>();
        services.AddHttpClient<FishAudioSpeechService>();
        services.AddScoped<ISpeechToTextService>(sp => sp.GetRequiredService<FishAudioSpeechService>());
        services.AddScoped<ITextToSpeechService>(sp => sp.GetRequiredService<FishAudioSpeechService>());

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString)));

        services.AddHangfireServer();
        services.AddSingleton<IReminderScheduler, HangfireReminderScheduler>();
        services.AddScoped<ReminderDeliveryJob>();
        services.AddScoped<MorningCheckInJob>();
        services.AddScoped<EveningReviewJob>();

        return services;
    }

    public static void ConfigureSerilog()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();
    }
}
