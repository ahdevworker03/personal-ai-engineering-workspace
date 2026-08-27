using Microsoft.EntityFrameworkCore;
using PersonalAssistant.Application.Abstractions;
using PersonalAssistant.Application.Common;
using PersonalAssistant.Domain;
using PersonalAssistant.Domain.Entities;
using PersonalAssistant.Domain.Enums;

namespace PersonalAssistant.Application.Goals;

public sealed class GoalService
{
    private readonly IAppDbContext _db;

    public GoalService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<GoalDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var goals = await _db.Goals
            .AsNoTracking()
            .Include(g => g.Tasks)
            .Where(g => g.ArchivedAt == null)
            .OrderByDescending(g => g.UpdatedAt)
            .ToListAsync(cancellationToken);

        return goals.Select(Map).ToList();
    }

    public async Task<GoalDto?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var goal = await _db.Goals
            .AsNoTracking()
            .Include(g => g.Tasks)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

        return goal is null ? null : Map(goal);
    }

    public async Task<GoalDto> CreateAsync(CreateGoalRequest request, CancellationToken cancellationToken = default)
    {
        var goal = new Goal
        {
            Title = request.Title.Trim(),
            Description = request.Description,
            Category = request.Category,
            Horizon = request.Horizon,
            Priority = request.Priority,
            StartDate = request.StartDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            TargetDate = request.TargetDate,
            SuccessCriteria = request.SuccessCriteria,
            Status = GoalStatus.Active
        };

        _db.Goals.Add(goal);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(goal);
    }

    public async Task<GoalDto?> UpdateAsync(Guid id, UpdateGoalRequest request, CancellationToken cancellationToken = default)
    {
        var goal = await _db.Goals.Include(g => g.Tasks).FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
        if (goal is null)
        {
            return null;
        }

        if (request.Title is not null) goal.Title = request.Title.Trim();
        if (request.Description is not null) goal.Description = request.Description;
        if (request.Category is not null) goal.Category = request.Category;
        if (request.Horizon is not null) goal.Horizon = request.Horizon.Value;
        if (request.Status is not null)
        {
            goal.Status = request.Status.Value;
            if (request.Status == GoalStatus.Completed)
            {
                goal.CompletedAt = DateTimeOffset.UtcNow;
                goal.ProgressPercent = 100;
            }
        }
        if (request.Priority is not null) goal.Priority = request.Priority.Value;
        if (request.StartDate is not null) goal.StartDate = request.StartDate;
        if (request.TargetDate is not null) goal.TargetDate = request.TargetDate;
        if (request.SuccessCriteria is not null) goal.SuccessCriteria = request.SuccessCriteria;
        if (request.ProgressPercent is not null)
        {
            goal.ProgressPercent = Math.Clamp(request.ProgressPercent.Value, 0, 100);
            goal.ProgressManuallyOverridden = true;
        }
        if (request.ProgressManuallyOverridden is not null)
        {
            goal.ProgressManuallyOverridden = request.ProgressManuallyOverridden.Value;
        }

        goal.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return Map(goal);
    }

    public async Task<bool> ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var goal = await _db.Goals.FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
        if (goal is null)
        {
            return false;
        }

        goal.Status = GoalStatus.Archived;
        goal.ArchivedAt = DateTimeOffset.UtcNow;
        goal.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task RecalculateProgressAsync(Guid goalId, CancellationToken cancellationToken = default)
    {
        var goal = await _db.Goals.Include(g => g.Tasks).FirstOrDefaultAsync(g => g.Id == goalId, cancellationToken);
        if (goal is null || goal.ProgressManuallyOverridden)
        {
            return;
        }

        goal.ProgressPercent = ProgressCalculator.CalculateGoalProgress(goal.Tasks);
        goal.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public static GoalDto Map(Goal goal)
    {
        if (!goal.ProgressManuallyOverridden && goal.Tasks.Count > 0)
        {
            goal.ProgressPercent = ProgressCalculator.CalculateGoalProgress(goal.Tasks);
        }

        return new GoalDto(
            goal.Id,
            goal.Title,
            goal.Description,
            goal.Category,
            goal.Horizon,
            goal.Status,
            goal.Priority,
            goal.StartDate,
            goal.TargetDate,
            goal.ProgressPercent,
            goal.ProgressManuallyOverridden,
            goal.SuccessCriteria,
            ProgressCalculator.CalculateTrackStatus(goal),
            goal.CreatedAt,
            goal.UpdatedAt,
            goal.CompletedAt);
    }
}
