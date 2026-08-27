using PersonalAssistant.Domain.Enums;

namespace PersonalAssistant.Application.Goals;

public sealed record CreateGoalRequest(
    string Title,
    string? Description,
    string? Category,
    GoalHorizon Horizon,
    int Priority,
    DateOnly? StartDate,
    DateOnly? TargetDate,
    string? SuccessCriteria);

public sealed record UpdateGoalRequest(
    string? Title,
    string? Description,
    string? Category,
    GoalHorizon? Horizon,
    GoalStatus? Status,
    int? Priority,
    DateOnly? StartDate,
    DateOnly? TargetDate,
    string? SuccessCriteria,
    decimal? ProgressPercent,
    bool? ProgressManuallyOverridden);
