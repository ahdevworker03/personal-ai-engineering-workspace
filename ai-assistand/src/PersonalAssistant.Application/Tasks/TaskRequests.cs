using PersonalAssistant.Domain.Enums;

namespace PersonalAssistant.Application.Tasks;

public sealed record CreateTaskRequest(
    string Title,
    string? Description,
    GoalTaskStatus? Status,
    int Priority,
    DateTimeOffset? DueAt,
    int? EstimatedMinutes,
    Guid? ParentTaskId);

public sealed record UpdateTaskRequest(
    string? Title,
    string? Description,
    GoalTaskStatus? Status,
    int? Priority,
    DateTimeOffset? DueAt,
    int? EstimatedMinutes,
    int? ActualMinutes,
    decimal? ProgressPercent,
    int? SortOrder);

public sealed record RecordProgressRequest(
    decimal? Percentage,
    decimal? ProgressDelta,
    string? Note,
    ProgressSource Source = ProgressSource.Manual);
