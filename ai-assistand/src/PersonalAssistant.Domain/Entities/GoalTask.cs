using PersonalAssistant.Domain.Enums;

namespace PersonalAssistant.Domain.Entities;

public class GoalTask
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GoalId { get; set; }
    public Guid? ParentTaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public GoalTaskStatus Status { get; set; } = GoalTaskStatus.Backlog;
    public int Priority { get; set; } = 3;
    public DateTimeOffset? DueAt { get; set; }
    public int? EstimatedMinutes { get; set; }
    public int? ActualMinutes { get; set; }
    public decimal ProgressPercent { get; set; }
    public int SortOrder { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset? ArchivedAt { get; set; }

    public Goal Goal { get; set; } = null!;
    public GoalTask? ParentTask { get; set; }
    public ICollection<GoalTask> Subtasks { get; set; } = new List<GoalTask>();
    public ICollection<ProgressEntry> ProgressEntries { get; set; } = new List<ProgressEntry>();
}
