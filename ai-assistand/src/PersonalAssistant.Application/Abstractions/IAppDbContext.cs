using Microsoft.EntityFrameworkCore;
using PersonalAssistant.Domain.Entities;

namespace PersonalAssistant.Application.Abstractions;

public interface IAppDbContext
{
    DbSet<UserProfile> UserProfiles { get; }
    DbSet<Goal> Goals { get; }
    DbSet<GoalTask> Tasks { get; }
    DbSet<ProgressEntry> ProgressEntries { get; }
    DbSet<Reminder> Reminders { get; }
    DbSet<Conversation> Conversations { get; }
    DbSet<ConversationMessage> ConversationMessages { get; }
    DbSet<MemoryItem> MemoryItems { get; }
    DbSet<CheckIn> CheckIns { get; }
    DbSet<AppNotification> Notifications { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
