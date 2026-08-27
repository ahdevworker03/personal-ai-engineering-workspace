using Microsoft.EntityFrameworkCore;
using PersonalAssistant.Application.Abstractions;
using PersonalAssistant.Application.Common;
using PersonalAssistant.Domain.Entities;
using PersonalAssistant.Domain.Enums;

namespace PersonalAssistant.Application.Reminders;

public sealed record CreateReminderRequest(
    string Title,
    string Message,
    DateTimeOffset ScheduledAt,
    Guid? GoalId,
    Guid? TaskId,
    string? RecurrenceRule);

public sealed record UpdateReminderRequest(
    string? Title,
    string? Message,
    DateTimeOffset? ScheduledAt,
    ReminderStatus? Status);

public sealed class ReminderService
{
    private readonly IAppDbContext _db;
    private readonly IReminderScheduler _scheduler;

    public ReminderService(IAppDbContext db, IReminderScheduler scheduler)
    {
        _db = db;
        _scheduler = scheduler;
    }

    public async Task<IReadOnlyList<ReminderDto>> ListAsync(DateTimeOffset? start = null, DateTimeOffset? end = null, CancellationToken cancellationToken = default)
    {
        var query = _db.Reminders.AsNoTracking().AsQueryable();
        if (start is not null) query = query.Where(r => r.ScheduledAt >= start);
        if (end is not null) query = query.Where(r => r.ScheduledAt < end);

        var items = await query.OrderBy(r => r.ScheduledAt).ToListAsync(cancellationToken);
        return items.Select(Map).ToList();
    }

    public async Task<ReminderDto> CreateAsync(CreateReminderRequest request, CancellationToken cancellationToken = default)
    {
        var reminder = new Reminder
        {
            Title = request.Title.Trim(),
            Message = request.Message.Trim(),
            ScheduledAt = request.ScheduledAt,
            GoalId = request.GoalId,
            TaskId = request.TaskId,
            RecurrenceRule = request.RecurrenceRule,
            Status = ReminderStatus.Pending
        };

        _db.Reminders.Add(reminder);
        await _db.SaveChangesAsync(cancellationToken);

        reminder.HangfireJobId = _scheduler.Schedule(reminder.Id, reminder.ScheduledAt);
        reminder.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return Map(reminder);
    }

    public async Task<ReminderDto?> UpdateAsync(Guid id, UpdateReminderRequest request, CancellationToken cancellationToken = default)
    {
        var reminder = await _db.Reminders.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (reminder is null) return null;

        if (request.Title is not null) reminder.Title = request.Title.Trim();
        if (request.Message is not null) reminder.Message = request.Message.Trim();
        if (request.Status is not null) reminder.Status = request.Status.Value;
        if (request.ScheduledAt is not null && request.ScheduledAt != reminder.ScheduledAt)
        {
            if (!string.IsNullOrWhiteSpace(reminder.HangfireJobId))
            {
                _scheduler.Cancel(reminder.HangfireJobId);
            }
            reminder.ScheduledAt = request.ScheduledAt.Value;
            reminder.HangfireJobId = _scheduler.Schedule(reminder.Id, reminder.ScheduledAt);
            reminder.Status = ReminderStatus.Pending;
        }

        reminder.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return Map(reminder);
    }

    public async Task<ReminderDto?> SnoozeAsync(Guid id, DateTimeOffset newScheduledAt, CancellationToken cancellationToken = default)
    {
        return await UpdateAsync(id, new UpdateReminderRequest(null, null, newScheduledAt, ReminderStatus.Snoozed), cancellationToken);
    }

    public async Task<bool> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var reminder = await _db.Reminders.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (reminder is null) return false;

        if (!string.IsNullOrWhiteSpace(reminder.HangfireJobId))
        {
            _scheduler.Cancel(reminder.HangfireJobId);
        }

        reminder.Status = ReminderStatus.Cancelled;
        reminder.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task DeliverAsync(Guid reminderId, CancellationToken cancellationToken = default)
    {
        var reminder = await _db.Reminders.FirstOrDefaultAsync(r => r.Id == reminderId, cancellationToken);
        if (reminder is null || reminder.Status == ReminderStatus.Cancelled)
        {
            return;
        }

        reminder.Status = ReminderStatus.Sent;
        reminder.UpdatedAt = DateTimeOffset.UtcNow;

        _db.Notifications.Add(new AppNotification
        {
            Title = reminder.Title,
            Message = reminder.Message,
            ReminderId = reminder.Id
        });

        await _db.SaveChangesAsync(cancellationToken);
    }

    private static ReminderDto Map(Reminder r) => new(r.Id, r.GoalId, r.TaskId, r.Title, r.Message, r.ScheduledAt, r.RecurrenceRule, r.Status, r.CreatedAt);
}
