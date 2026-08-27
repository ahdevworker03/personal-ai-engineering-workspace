using Microsoft.EntityFrameworkCore;
using PersonalAssistant.Application.Abstractions;
using PersonalAssistant.Domain.Entities;

namespace PersonalAssistant.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Goal> Goals => Set<Goal>();
    public DbSet<GoalTask> Tasks => Set<GoalTask>();
    public DbSet<ProgressEntry> ProgressEntries => Set<ProgressEntry>();
    public DbSet<Reminder> Reminders => Set<Reminder>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ConversationMessage> ConversationMessages => Set<ConversationMessage>();
    public DbSet<MemoryItem> MemoryItems => Set<MemoryItem>();
    public DbSet<CheckIn> CheckIns => Set<CheckIn>();
    public DbSet<AppNotification> Notifications => Set<AppNotification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Goal>(e =>
        {
            e.HasIndex(x => x.Status);
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.ProgressPercent).HasPrecision(5, 2);
        });

        modelBuilder.Entity<GoalTask>(e =>
        {
            e.ToTable("Tasks");
            e.HasIndex(x => x.GoalId);
            e.HasIndex(x => x.DueAt);
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.ProgressPercent).HasPrecision(5, 2);
            e.HasOne(x => x.Goal).WithMany(x => x.Tasks).HasForeignKey(x => x.GoalId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.ParentTask).WithMany(x => x.Subtasks).HasForeignKey(x => x.ParentTaskId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProgressEntry>(e =>
        {
            e.Property(x => x.ProgressDelta).HasPrecision(5, 2);
            e.Property(x => x.ProgressPercentAfter).HasPrecision(5, 2);
            e.HasOne(x => x.Goal).WithMany(x => x.ProgressEntries).HasForeignKey(x => x.GoalId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Task).WithMany(x => x.ProgressEntries).HasForeignKey(x => x.TaskId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Reminder>(e =>
        {
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.HasIndex(x => x.ScheduledAt);
        });

        modelBuilder.Entity<MemoryItem>(e =>
        {
            e.HasIndex(x => x.Key).IsUnique();
            e.Property(x => x.Key).HasMaxLength(120).IsRequired();
        });

        modelBuilder.Entity<ConversationMessage>(e =>
        {
            e.HasOne(x => x.Conversation).WithMany(x => x.Messages).HasForeignKey(x => x.ConversationId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
