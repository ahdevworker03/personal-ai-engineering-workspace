using PersonalAssistant.Domain.Enums;

namespace PersonalAssistant.Domain.Entities;

public class MemoryItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public MemoryType Type { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int Importance { get; set; } = 1;
    public DateTimeOffset? ExpiresAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
