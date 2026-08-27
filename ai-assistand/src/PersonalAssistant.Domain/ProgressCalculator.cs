using PersonalAssistant.Domain.Entities;
using PersonalAssistant.Domain.Enums;

namespace PersonalAssistant.Domain;

public static class ProgressCalculator
{
    public static decimal CalculateGoalProgress(IEnumerable<GoalTask> tasks)
    {
        var active = tasks
            .Where(t => t.Status != GoalTaskStatus.Cancelled && t.ArchivedAt is null)
            .ToList();

        if (active.Count == 0)
        {
            return 0m;
        }

        decimal totalWeight = 0m;
        decimal weighted = 0m;

        foreach (var task in active)
        {
            var weight = task.EstimatedMinutes is > 0 ? task.EstimatedMinutes.Value : 1m;
            totalWeight += weight;
            weighted += weight * task.ProgressPercent;
        }

        if (totalWeight <= 0)
        {
            return 0m;
        }

        return Math.Round(weighted / totalWeight, 2);
    }

    public static TrackStatus CalculateTrackStatus(Goal goal)
    {
        if (goal.StartDate is null || goal.TargetDate is null)
        {
            return TrackStatus.Unknown;
        }

        var start = goal.StartDate.Value.ToDateTime(TimeOnly.MinValue);
        var target = goal.TargetDate.Value.ToDateTime(TimeOnly.MinValue);
        var totalDays = (target - start).TotalDays;

        if (totalDays <= 0)
        {
            return TrackStatus.Unknown;
        }

        var elapsedDays = (DateTime.UtcNow.Date - start).TotalDays;
        var expected = (decimal)(Math.Clamp(elapsedDays / totalDays, 0, 1) * 100);
        var delta = goal.ProgressPercent - expected;

        if (delta >= 10)
        {
            return TrackStatus.Ahead;
        }

        if (delta >= -10)
        {
            return TrackStatus.OnTrack;
        }

        if (delta >= -25)
        {
            return TrackStatus.AtRisk;
        }

        return TrackStatus.OffTrack;
    }
}
