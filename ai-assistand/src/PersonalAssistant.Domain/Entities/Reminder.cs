using PersonalAssistant.Domain.Enums;

namespace PersonalAssistant.Domain.Entities;

public class Reminder
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? GoalId { get; set; }
    public Guid? TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTimeOffset ScheduledAt { get; set; }
    public string? RecurrenceRule { get; set; }
    public ReminderStatus Status { get; set; } = ReminderStatus.Pending;
    public string? HangfireJobId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Goal? Goal { get; set; }
    public GoalTask? Task { get; set; }
}
