using PersonalAssistant.Domain;
using PersonalAssistant.Domain.Entities;
using PersonalAssistant.Domain.Enums;

namespace PersonalAssistant.UnitTests;

public class ProgressCalculatorTests
{
    [Fact]
    public void CalculateGoalProgress_UsesEstimatedMinutesAsWeight()
    {
        var tasks = new List<GoalTask>
        {
            new() { EstimatedMinutes = 60, ProgressPercent = 100, Status = GoalTaskStatus.Done },
            new() { EstimatedMinutes = 60, ProgressPercent = 0, Status = GoalTaskStatus.Ready }
        };

        var progress = ProgressCalculator.CalculateGoalProgress(tasks);
        Assert.Equal(50m, progress);
    }

    [Fact]
    public void CalculateTrackStatus_ReturnsOnTrackNearExpected()
    {
        var goal = new Goal
        {
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-50)),
            TargetDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(50)),
            ProgressPercent = 50
        };

        var status = ProgressCalculator.CalculateTrackStatus(goal);
        Assert.Equal(TrackStatus.OnTrack, status);
    }

    [Fact]
    public void CalculateTrackStatus_UnknownWithoutDates()
    {
        var goal = new Goal { ProgressPercent = 40 };
        Assert.Equal(TrackStatus.Unknown, ProgressCalculator.CalculateTrackStatus(goal));
    }
}
