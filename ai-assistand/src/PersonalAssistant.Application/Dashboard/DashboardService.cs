using Microsoft.EntityFrameworkCore;
using PersonalAssistant.Application.Abstractions;
using PersonalAssistant.Application.Common;
using PersonalAssistant.Application.Goals;
using PersonalAssistant.Application.Tasks;
using PersonalAssistant.Domain.Enums;

namespace PersonalAssistant.Application.Dashboard;

public sealed class DashboardService
{
    private readonly IAppDbContext _db;
    private readonly GoalService _goals;
    private readonly TaskService _tasks;

    public DashboardService(IAppDbContext db, GoalService goals, TaskService tasks)
    {
        _db = db;
        _goals = goals;
        _tasks = tasks;
    }

    public async Task<DashboardDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var profile = await _db.UserProfiles.AsNoTracking().OrderBy(p => p.CreatedAt).FirstOrDefaultAsync(cancellationToken);
        var name = profile?.DisplayName ?? "there";
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var start = DateTimeOffset.UtcNow.Date;
        var end = start.AddDays(1);

        var activeGoals = (await _goals.ListAsync(cancellationToken))
            .Where(g => g.Status == GoalStatus.Active)
            .ToList();

        var dueToday = await _tasks.GetDueBetweenAsync(start, end, cancellationToken);
        var overdue = await _tasks.GetOverdueAsync(cancellationToken);

        var nextReminder = await _db.Reminders.AsNoTracking()
            .Where(r => r.Status == ReminderStatus.Pending && r.ScheduledAt >= DateTimeOffset.UtcNow)
            .OrderBy(r => r.ScheduledAt)
            .FirstOrDefaultAsync(cancellationToken);

        var weekAgo = DateTimeOffset.UtcNow.AddDays(-7);
        var recent = await _db.ProgressEntries.AsNoTracking()
            .Where(p => p.CreatedAt >= weekAgo)
            .OrderByDescending(p => p.CreatedAt)
            .Take(10)
            .ToListAsync(cancellationToken);

        var summary = recent.Count == 0
            ? "No progress recorded in the last 7 days."
            : $"{recent.Count} progress updates in the last 7 days. Latest: {recent[0].Note ?? $"{recent[0].ProgressPercentAfter}%"}.";

        ReminderDto? reminderDto = nextReminder is null
            ? null
            : new ReminderDto(nextReminder.Id, nextReminder.GoalId, nextReminder.TaskId, nextReminder.Title, nextReminder.Message, nextReminder.ScheduledAt, nextReminder.RecurrenceRule, nextReminder.Status, nextReminder.CreatedAt);

        return new DashboardDto(
            $"Hello, {name}",
            today,
            activeGoals,
            dueToday,
            overdue,
            reminderDto,
            summary);
    }
}
