using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PersonalAssistant.Application.Abstractions;
using PersonalAssistant.Application.Goals;
using PersonalAssistant.Application.Reminders;
using PersonalAssistant.Application.Tasks;
using PersonalAssistant.Domain.Enums;

namespace PersonalAssistant.Application.Assistant;

public sealed class AssistantToolDispatcher
{
    private readonly GoalService _goals;
    private readonly TaskService _tasks;
    private readonly ReminderService _reminders;
    private readonly IAppDbContext _db;

    public AssistantToolDispatcher(GoalService goals, TaskService tasks, ReminderService reminders, IAppDbContext db)
    {
        _goals = goals;
        _tasks = tasks;
        _reminders = reminders;
        _db = db;
    }

    public async Task<string> DispatchAsync(string name, string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
            var root = doc.RootElement;

            return name switch
            {
                "get_dashboard_summary" => JsonSerializer.Serialize(await BuildDashboardSummary(cancellationToken)),
                "get_active_goals" => JsonSerializer.Serialize(await _goals.ListAsync(cancellationToken)),
                "get_goal" => JsonSerializer.Serialize(await _goals.GetAsync(GetGuid(root, "goalId"), cancellationToken)),
                "get_goal_tasks" => JsonSerializer.Serialize(await _tasks.ListByGoalAsync(
                    GetGuid(root, "goalId"),
                    root.TryGetProperty("status", out var statusEl) && Enum.TryParse<GoalTaskStatus>(statusEl.GetString(), true, out var st) ? st : null,
                    cancellationToken)),
                "get_tasks_due_between" => JsonSerializer.Serialize(await _tasks.GetDueBetweenAsync(GetDate(root, "start"), GetDate(root, "end"), cancellationToken)),
                "get_overdue_tasks" => JsonSerializer.Serialize(await _tasks.GetOverdueAsync(cancellationToken)),
                "get_recent_progress" => await GetRecentProgressAsync(root, cancellationToken),
                "get_reminders" => JsonSerializer.Serialize(await _reminders.ListAsync(GetOptionalDate(root, "start"), GetOptionalDate(root, "end"), cancellationToken)),
                "create_goal" => JsonSerializer.Serialize(await _goals.CreateAsync(new CreateGoalRequest(
                    GetString(root, "title"),
                    GetOptionalString(root, "description"),
                    null,
                    Enum.TryParse<GoalHorizon>(GetOptionalString(root, "horizon"), true, out var horizon) ? horizon : GoalHorizon.NearTerm,
                    3,
                    null,
                    GetOptionalDateOnly(root, "targetDate"),
                    GetOptionalString(root, "successCriteria")), cancellationToken)),
                "update_goal" => JsonSerializer.Serialize(await _goals.UpdateAsync(GetGuid(root, "goalId"), new UpdateGoalRequest(
                    GetOptionalString(root, "title"),
                    GetOptionalString(root, "description"),
                    null,
                    null,
                    Enum.TryParse<GoalStatus>(GetOptionalString(root, "status"), true, out var gs) ? gs : null,
                    null,
                    null,
                    GetOptionalDateOnly(root, "targetDate"),
                    GetOptionalString(root, "successCriteria"),
                    null,
                    null), cancellationToken)),
                "create_task" => JsonSerializer.Serialize(await _tasks.CreateAsync(GetGuid(root, "goalId"), new CreateTaskRequest(
                    GetString(root, "title"),
                    GetOptionalString(root, "description"),
                    null,
                    root.TryGetProperty("priority", out var p) ? p.GetInt32() : 3,
                    GetOptionalDate(root, "dueAt"),
                    root.TryGetProperty("estimatedMinutes", out var em) ? em.GetInt32() : null,
                    null), cancellationToken)),
                "update_task" => JsonSerializer.Serialize(await _tasks.UpdateAsync(GetGuid(root, "taskId"), new UpdateTaskRequest(
                    GetOptionalString(root, "title"),
                    GetOptionalString(root, "description"),
                    null,
                    root.TryGetProperty("priority", out var up) ? up.GetInt32() : null,
                    GetOptionalDate(root, "dueAt"),
                    null,
                    null,
                    root.TryGetProperty("progressPercent", out var pp) ? pp.GetDecimal() : null,
                    null), cancellationToken)),
                "set_task_status" => JsonSerializer.Serialize(await _tasks.UpdateAsync(GetGuid(root, "taskId"), new UpdateTaskRequest(
                    null, null,
                    Enum.Parse<GoalTaskStatus>(GetString(root, "status"), true),
                    null, null, null, null, null, null), cancellationToken)),
                "record_progress" => await RecordProgressToolAsync(root, cancellationToken),
                "create_reminder" => JsonSerializer.Serialize(await _reminders.CreateAsync(new CreateReminderRequest(
                    GetOptionalString(root, "title") ?? "Reminder",
                    GetString(root, "message"),
                    GetDate(root, "scheduledAt"),
                    GetOptionalGuid(root, "goalId"),
                    GetOptionalGuid(root, "taskId"),
                    GetOptionalString(root, "recurrenceRule")), cancellationToken)),
                "snooze_reminder" => JsonSerializer.Serialize(await _reminders.SnoozeAsync(GetGuid(root, "reminderId"), GetDate(root, "newScheduledAt"), cancellationToken)),
                _ => JsonSerializer.Serialize(new { error = $"Unknown tool: {name}" })
            };
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    private async Task<object> BuildDashboardSummary(CancellationToken cancellationToken)
    {
        var goals = (await _goals.ListAsync(cancellationToken)).Where(g => g.Status == GoalStatus.Active).ToList();
        var overdue = await _tasks.GetOverdueAsync(cancellationToken);
        var start = DateTimeOffset.UtcNow.Date;
        var dueToday = await _tasks.GetDueBetweenAsync(start, start.AddDays(1), cancellationToken);
        return new
        {
            activeGoalCount = goals.Count,
            dueTodayCount = dueToday.Count,
            overdueCount = overdue.Count,
            goals,
            dueToday,
            overdue
        };
    }

    private async Task<string> GetRecentProgressAsync(JsonElement root, CancellationToken cancellationToken)
    {
        var days = root.TryGetProperty("days", out var d) ? d.GetInt32() : 7;
        var since = DateTimeOffset.UtcNow.AddDays(-days);
        var query = _db.ProgressEntries.AsQueryable().Where(p => p.CreatedAt >= since);
        if (root.TryGetProperty("goalId", out var g) && g.ValueKind != JsonValueKind.Null)
        {
            var goalId = g.GetGuid();
            query = query.Where(p => p.GoalId == goalId);
        }

        var items = await query.OrderByDescending(p => p.CreatedAt).Take(20).ToListAsync(cancellationToken);
        return JsonSerializer.Serialize(items);
    }

    private async Task<string> RecordProgressToolAsync(JsonElement root, CancellationToken cancellationToken)
    {
        var taskId = GetOptionalGuid(root, "taskId");
        if (taskId is null)
        {
            return JsonSerializer.Serialize(new { error = "taskId is required for record_progress in MVP." });
        }

        var result = await _tasks.RecordProgressAsync(taskId.Value, new RecordProgressRequest(
            root.TryGetProperty("percentage", out var pct) ? pct.GetDecimal() : null,
            null,
            GetOptionalString(root, "note"),
            ProgressSource.Assistant), cancellationToken);

        return JsonSerializer.Serialize(result);
    }

    private static Guid GetGuid(JsonElement root, string name) => root.GetProperty(name).GetGuid();
    private static Guid? GetOptionalGuid(JsonElement root, string name) =>
        root.TryGetProperty(name, out var el) && el.ValueKind != JsonValueKind.Null ? el.GetGuid() : null;
    private static string GetString(JsonElement root, string name) => root.GetProperty(name).GetString() ?? string.Empty;
    private static string? GetOptionalString(JsonElement root, string name) =>
        root.TryGetProperty(name, out var el) && el.ValueKind != JsonValueKind.Null ? el.GetString() : null;
    private static DateTimeOffset GetDate(JsonElement root, string name) => DateTimeOffset.Parse(root.GetProperty(name).GetString()!);
    private static DateTimeOffset? GetOptionalDate(JsonElement root, string name) =>
        root.TryGetProperty(name, out var el) && el.ValueKind != JsonValueKind.Null ? DateTimeOffset.Parse(el.GetString()!) : null;
    private static DateOnly? GetOptionalDateOnly(JsonElement root, string name) =>
        root.TryGetProperty(name, out var el) && el.ValueKind != JsonValueKind.Null ? DateOnly.Parse(el.GetString()!) : null;
}
