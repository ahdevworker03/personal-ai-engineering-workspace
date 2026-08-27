using PersonalAssistant.Domain.Enums;

namespace PersonalAssistant.Application.Common;

public sealed record GoalDto(
    Guid Id,
    string Title,
    string? Description,
    string? Category,
    GoalHorizon Horizon,
    GoalStatus Status,
    int Priority,
    DateOnly? StartDate,
    DateOnly? TargetDate,
    decimal ProgressPercent,
    bool ProgressManuallyOverridden,
    string? SuccessCriteria,
    TrackStatus TrackStatus,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? CompletedAt);

public sealed record TaskDto(
    Guid Id,
    Guid GoalId,
    Guid? ParentTaskId,
    string Title,
    string? Description,
    GoalTaskStatus Status,
    int Priority,
    DateTimeOffset? DueAt,
    int? EstimatedMinutes,
    int? ActualMinutes,
    decimal ProgressPercent,
    int SortOrder,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? CompletedAt);

public sealed record ProgressEntryDto(
    Guid Id,
    Guid? GoalId,
    Guid? TaskId,
    decimal ProgressDelta,
    decimal ProgressPercentAfter,
    string? Note,
    ProgressSource Source,
    DateTimeOffset CreatedAt);

public sealed record ReminderDto(
    Guid Id,
    Guid? GoalId,
    Guid? TaskId,
    string Title,
    string Message,
    DateTimeOffset ScheduledAt,
    string? RecurrenceRule,
    ReminderStatus Status,
    DateTimeOffset CreatedAt);

public sealed record ConversationDto(
    Guid Id,
    string Title,
    string? Summary,
    DateTimeOffset StartedAt,
    DateTimeOffset LastMessageAt);

public sealed record ConversationMessageDto(
    Guid Id,
    Guid ConversationId,
    MessageRole Role,
    string Content,
    string? ToolName,
    DateTimeOffset CreatedAt);

public sealed record MemoryItemDto(
    Guid Id,
    MemoryType Type,
    string Key,
    string Value,
    int Importance,
    DateTimeOffset? ExpiresAt);

public sealed record NotificationDto(
    Guid Id,
    string Title,
    string Message,
    Guid? ReminderId,
    bool IsRead,
    DateTimeOffset CreatedAt);

public sealed record DashboardDto(
    string Greeting,
    DateOnly Today,
    IReadOnlyList<GoalDto> ActiveGoals,
    IReadOnlyList<TaskDto> TasksDueToday,
    IReadOnlyList<TaskDto> OverdueTasks,
    ReminderDto? NextReminder,
    string RecentProgressSummary);
