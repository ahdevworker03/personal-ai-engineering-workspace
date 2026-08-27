using Microsoft.EntityFrameworkCore;
using PersonalAssistant.Application.Abstractions;
using PersonalAssistant.Application.Common;
using PersonalAssistant.Application.Goals;
using PersonalAssistant.Domain.Entities;
using PersonalAssistant.Domain.Enums;

namespace PersonalAssistant.Application.Tasks;

public sealed class TaskService
{
    private readonly IAppDbContext _db;
    private readonly GoalService _goals;

    public TaskService(IAppDbContext db, GoalService goals)
    {
        _db = db;
        _goals = goals;
    }

    public async Task<IReadOnlyList<TaskDto>> ListByGoalAsync(Guid goalId, GoalTaskStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = _db.Tasks.AsNoTracking().Where(t => t.GoalId == goalId && t.ArchivedAt == null);
        if (status is not null)
        {
            query = query.Where(t => t.Status == status);
        }

        var tasks = await query.OrderBy(t => t.SortOrder).ThenBy(t => t.CreatedAt).ToListAsync(cancellationToken);
        return tasks.Select(Map).ToList();
    }

    public async Task<TaskDto?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await _db.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        return task is null ? null : Map(task);
    }

    public async Task<TaskDto> CreateAsync(Guid goalId, CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var goalExists = await _db.Goals.AnyAsync(g => g.Id == goalId, cancellationToken);
        if (!goalExists)
        {
            throw new InvalidOperationException("Goal not found.");
        }

        var maxSort = await _db.Tasks.Where(t => t.GoalId == goalId).Select(t => (int?)t.SortOrder).MaxAsync(cancellationToken) ?? 0;
        var task = new GoalTask
        {
            GoalId = goalId,
            ParentTaskId = request.ParentTaskId,
            Title = request.Title.Trim(),
            Description = request.Description,
            Status = request.Status ?? GoalTaskStatus.Ready,
            Priority = request.Priority,
            DueAt = request.DueAt,
            EstimatedMinutes = request.EstimatedMinutes,
            SortOrder = maxSort + 1
        };

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync(cancellationToken);
        await _goals.RecalculateProgressAsync(goalId, cancellationToken);
        return Map(task);
    }

    public async Task<TaskDto?> UpdateAsync(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (task is null)
        {
            return null;
        }

        if (request.Title is not null) task.Title = request.Title.Trim();
        if (request.Description is not null) task.Description = request.Description;
        if (request.Priority is not null) task.Priority = request.Priority.Value;
        if (request.DueAt is not null) task.DueAt = request.DueAt;
        if (request.EstimatedMinutes is not null) task.EstimatedMinutes = request.EstimatedMinutes;
        if (request.ActualMinutes is not null) task.ActualMinutes = request.ActualMinutes;
        if (request.SortOrder is not null) task.SortOrder = request.SortOrder.Value;
        if (request.ProgressPercent is not null)
        {
            task.ProgressPercent = Math.Clamp(request.ProgressPercent.Value, 0, 100);
        }
        if (request.Status is not null)
        {
            ApplyStatus(task, request.Status.Value);
        }

        task.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        await _goals.RecalculateProgressAsync(task.GoalId, cancellationToken);
        return Map(task);
    }

    public async Task<TaskDto?> CompleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await UpdateAsync(id, new UpdateTaskRequest(null, null, GoalTaskStatus.Done, null, null, null, null, 100, null), cancellationToken);
    }

    public async Task<ProgressEntryDto> RecordProgressAsync(Guid taskId, RecordProgressRequest request, CancellationToken cancellationToken = default)
    {
        var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken)
            ?? throw new InvalidOperationException("Task not found.");

        var before = task.ProgressPercent;
        decimal after;
        if (request.Percentage is not null)
        {
            after = Math.Clamp(request.Percentage.Value, 0, 100);
        }
        else if (request.ProgressDelta is not null)
        {
            after = Math.Clamp(before + request.ProgressDelta.Value, 0, 100);
        }
        else
        {
            throw new InvalidOperationException("Provide percentage or progressDelta.");
        }

        task.ProgressPercent = after;
        if (after >= 100)
        {
            ApplyStatus(task, GoalTaskStatus.Done);
        }
        else if (task.Status is GoalTaskStatus.Backlog or GoalTaskStatus.Ready)
        {
            task.Status = GoalTaskStatus.InProgress;
        }

        task.UpdatedAt = DateTimeOffset.UtcNow;

        var entry = new ProgressEntry
        {
            GoalId = task.GoalId,
            TaskId = task.Id,
            ProgressDelta = after - before,
            ProgressPercentAfter = after,
            Note = request.Note,
            Source = request.Source
        };

        _db.ProgressEntries.Add(entry);
        await _db.SaveChangesAsync(cancellationToken);
        await _goals.RecalculateProgressAsync(task.GoalId, cancellationToken);

        return new ProgressEntryDto(entry.Id, entry.GoalId, entry.TaskId, entry.ProgressDelta, entry.ProgressPercentAfter, entry.Note, entry.Source, entry.CreatedAt);
    }

    public async Task<IReadOnlyList<TaskDto>> GetDueBetweenAsync(DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken = default)
    {
        var tasks = await _db.Tasks.AsNoTracking()
            .Where(t => t.ArchivedAt == null && t.DueAt != null && t.DueAt >= start && t.DueAt < end && t.Status != GoalTaskStatus.Done && t.Status != GoalTaskStatus.Cancelled)
            .OrderBy(t => t.DueAt)
            .ToListAsync(cancellationToken);
        return tasks.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<TaskDto>> GetOverdueAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var tasks = await _db.Tasks.AsNoTracking()
            .Where(t => t.ArchivedAt == null && t.DueAt != null && t.DueAt < now && t.Status != GoalTaskStatus.Done && t.Status != GoalTaskStatus.Cancelled)
            .OrderBy(t => t.DueAt)
            .ToListAsync(cancellationToken);
        return tasks.Select(Map).ToList();
    }

    private static void ApplyStatus(GoalTask task, GoalTaskStatus status)
    {
        task.Status = status;
        if (status == GoalTaskStatus.Done)
        {
            task.ProgressPercent = 100;
            task.CompletedAt = DateTimeOffset.UtcNow;
        }
        else if (task.CompletedAt is not null)
        {
            task.CompletedAt = null;
        }
    }

    public static TaskDto Map(GoalTask task) => new(
        task.Id,
        task.GoalId,
        task.ParentTaskId,
        task.Title,
        task.Description,
        task.Status,
        task.Priority,
        task.DueAt,
        task.EstimatedMinutes,
        task.ActualMinutes,
        task.ProgressPercent,
        task.SortOrder,
        task.CreatedAt,
        task.UpdatedAt,
        task.CompletedAt);
}
