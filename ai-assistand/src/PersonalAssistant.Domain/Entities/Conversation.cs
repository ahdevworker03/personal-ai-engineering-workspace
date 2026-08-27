namespace PersonalAssistant.Domain.Entities;

public class Conversation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "New conversation";
    public string? Summary { get; set; }
    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastMessageAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<ConversationMessage> Messages { get; set; } = new List<ConversationMessage>();
}
