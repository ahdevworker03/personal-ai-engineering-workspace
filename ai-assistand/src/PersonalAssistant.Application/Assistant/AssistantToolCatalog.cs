using PersonalAssistant.Application.Abstractions;

namespace PersonalAssistant.Application.Assistant;

public static class AssistantToolCatalog
{
    public static IReadOnlyList<ToolDefinitionDto> Definitions { get; } =
    [
        Tool("get_dashboard_summary", "Get counts and lists for active goals, due today, and overdue tasks.", "{}"),
        Tool("get_active_goals", "List non-archived goals with progress and track status.", "{}"),
        Tool("get_goal", "Get one goal by id.", """{"type":"object","properties":{"goalId":{"type":"string","format":"uuid"}},"required":["goalId"]}"""),
        Tool("get_goal_tasks", "List tasks for a goal.", """{"type":"object","properties":{"goalId":{"type":"string","format":"uuid"},"status":{"type":"string"}},"required":["goalId"]}"""),
        Tool("get_tasks_due_between", "List tasks due between start and end timestamps.", """{"type":"object","properties":{"start":{"type":"string"},"end":{"type":"string"}},"required":["start","end"]}"""),
        Tool("get_overdue_tasks", "List overdue incomplete tasks.", "{}"),
        Tool("get_recent_progress", "List recent progress entries.", """{"type":"object","properties":{"goalId":{"type":"string","format":"uuid"},"days":{"type":"integer"}}}"""),
        Tool("get_reminders", "List reminders in an optional time window.", """{"type":"object","properties":{"start":{"type":"string"},"end":{"type":"string"}}}"""),
        Tool("create_goal", "Create a goal.", """{"type":"object","properties":{"title":{"type":"string"},"description":{"type":"string"},"horizon":{"type":"string"},"targetDate":{"type":"string"},"successCriteria":{"type":"string"}},"required":["title"]}"""),
        Tool("update_goal", "Update goal fields.", """{"type":"object","properties":{"goalId":{"type":"string","format":"uuid"},"title":{"type":"string"},"description":{"type":"string"},"status":{"type":"string"},"targetDate":{"type":"string"},"successCriteria":{"type":"string"}},"required":["goalId"]}"""),
        Tool("create_task", "Create a task under a goal.", """{"type":"object","properties":{"goalId":{"type":"string","format":"uuid"},"title":{"type":"string"},"description":{"type":"string"},"priority":{"type":"integer"},"dueAt":{"type":"string"},"estimatedMinutes":{"type":"integer"}},"required":["goalId","title"]}"""),
        Tool("update_task", "Update task fields.", """{"type":"object","properties":{"taskId":{"type":"string","format":"uuid"},"title":{"type":"string"},"description":{"type":"string"},"priority":{"type":"integer"},"dueAt":{"type":"string"},"progressPercent":{"type":"number"}},"required":["taskId"]}"""),
        Tool("set_task_status", "Set task status (Backlog, Ready, InProgress, Blocked, Done, Cancelled).", """{"type":"object","properties":{"taskId":{"type":"string","format":"uuid"},"status":{"type":"string"}},"required":["taskId","status"]}"""),
        Tool("record_progress", "Record progress for a task.", """{"type":"object","properties":{"taskId":{"type":"string","format":"uuid"},"percentage":{"type":"number"},"note":{"type":"string"}},"required":["taskId"]}"""),
        Tool("create_reminder", "Create a reminder.", """{"type":"object","properties":{"title":{"type":"string"},"message":{"type":"string"},"scheduledAt":{"type":"string"},"goalId":{"type":"string","format":"uuid"},"taskId":{"type":"string","format":"uuid"},"recurrenceRule":{"type":"string"}},"required":["message","scheduledAt"]}"""),
        Tool("snooze_reminder", "Snooze a reminder to a new time.", """{"type":"object","properties":{"reminderId":{"type":"string","format":"uuid"},"newScheduledAt":{"type":"string"}},"required":["reminderId","newScheduledAt"]}""")
    ];

    private static ToolDefinitionDto Tool(string name, string description, string schema) =>
        new()
        {
            Name = name,
            Description = description,
            ParametersJsonSchema = schema == "{}"
                ? """{"type":"object","properties":{}}"""
                : schema
        };
}
