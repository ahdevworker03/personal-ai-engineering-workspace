using PersonalAssistant.Domain.Enums;

namespace PersonalAssistant.Domain.Entities;

public class CheckIn
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public CheckInType Type { get; set; }
    public DateTimeOffset ScheduledAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string? Mood { get; set; }
    public string? Summary { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
