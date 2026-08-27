using PersonalAssistant.Domain.Enums;

namespace PersonalAssistant.Domain.Entities;

public class ConversationMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ConversationId { get; set; }
    public MessageRole Role { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? ToolName { get; set; }
    public string? ToolCallId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Conversation Conversation { get; set; } = null!;
}
