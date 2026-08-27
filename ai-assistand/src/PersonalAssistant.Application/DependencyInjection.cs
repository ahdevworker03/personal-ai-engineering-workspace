using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PersonalAssistant.Application.Assistant;
using PersonalAssistant.Application.Dashboard;
using PersonalAssistant.Application.Goals;
using PersonalAssistant.Application.Memory;
using PersonalAssistant.Application.Reminders;
using PersonalAssistant.Application.Tasks;

namespace PersonalAssistant.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateGoalValidator>();
        services.AddScoped<GoalService>();
        services.AddScoped<TaskService>();
        services.AddScoped<DashboardService>();
        services.AddScoped<ReminderService>();
        services.AddScoped<MemoryService>();
        services.AddScoped<AssistantToolDispatcher>();
        services.AddScoped<AssistantOrchestrator>();
        return services;
    }
}

public sealed class CreateGoalValidator : AbstractValidator<CreateGoalRequest>
{
    public CreateGoalValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Priority).InclusiveBetween(1, 5);
    }
}

public sealed class CreateTaskValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Priority).InclusiveBetween(1, 5);
    }
}

public sealed class CreateReminderValidator : AbstractValidator<CreateReminderRequest>
{
    public CreateReminderValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Message).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.ScheduledAt).Must(d => d > DateTimeOffset.UtcNow.AddMinutes(-1));
    }
}
