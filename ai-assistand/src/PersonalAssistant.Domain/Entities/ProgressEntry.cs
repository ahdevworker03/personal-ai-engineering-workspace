using PersonalAssistant.Domain.Enums;

namespace PersonalAssistant.Domain.Entities;

public class ProgressEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? GoalId { get; set; }
    public Guid? TaskId { get; set; }
    public decimal ProgressDelta { get; set; }
    public decimal ProgressPercentAfter { get; set; }
    public string? Note { get; set; }
    public ProgressSource Source { get; set; } = ProgressSource.Manual;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Goal? Goal { get; set; }
    public GoalTask? Task { get; set; }
}
