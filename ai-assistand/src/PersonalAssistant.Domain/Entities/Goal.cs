using PersonalAssistant.Domain.Enums;

namespace PersonalAssistant.Domain.Entities;

public class Goal
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public GoalHorizon Horizon { get; set; } = GoalHorizon.NearTerm;
    public GoalStatus Status { get; set; } = GoalStatus.Active;
    public int Priority { get; set; } = 3;
    public DateOnly? StartDate { get; set; }
    public DateOnly? TargetDate { get; set; }
    public decimal ProgressPercent { get; set; }
    public bool ProgressManuallyOverridden { get; set; }
    public string? SuccessCriteria { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset? ArchivedAt { get; set; }

    public ICollection<GoalTask> Tasks { get; set; } = new List<GoalTask>();
    public ICollection<ProgressEntry> ProgressEntries { get; set; } = new List<ProgressEntry>();
}
