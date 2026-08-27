using Microsoft.EntityFrameworkCore;
using PersonalAssistant.Application.Abstractions;
using PersonalAssistant.Application.Common;
using PersonalAssistant.Domain.Entities;
using PersonalAssistant.Domain.Enums;

namespace PersonalAssistant.Application.Memory;

public sealed record UpsertMemoryRequest(MemoryType Type, string Key, string Value, int Importance, DateTimeOffset? ExpiresAt);
public sealed record UpdateProfileRequest(string? DisplayName, string? TimeZone, string? PreferredLanguage, string? PreferredTone, TimeOnly? DailyCheckInTime);

public sealed class MemoryService
{
    private readonly IAppDbContext _db;

    public MemoryService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<MemoryItemDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var items = await _db.MemoryItems.AsNoTracking()
            .Where(m => m.ExpiresAt == null || m.ExpiresAt > DateTimeOffset.UtcNow)
            .OrderByDescending(m => m.Importance)
            .ThenByDescending(m => m.UpdatedAt)
            .ToListAsync(cancellationToken);

        return items.Select(m => new MemoryItemDto(m.Id, m.Type, m.Key, m.Value, m.Importance, m.ExpiresAt)).ToList();
    }

    public async Task<MemoryItemDto> UpsertAsync(UpsertMemoryRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _db.MemoryItems.FirstOrDefaultAsync(m => m.Key == request.Key, cancellationToken);
        if (existing is null)
        {
            existing = new MemoryItem
            {
                Type = request.Type,
                Key = request.Key,
                Value = request.Value,
                Importance = request.Importance,
                ExpiresAt = request.ExpiresAt
            };
            _db.MemoryItems.Add(existing);
        }
        else
        {
            existing.Type = request.Type;
            existing.Value = request.Value;
            existing.Importance = request.Importance;
            existing.ExpiresAt = request.ExpiresAt;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return new MemoryItemDto(existing.Id, existing.Type, existing.Key, existing.Value, existing.Importance, existing.ExpiresAt);
    }

    public async Task<UserProfile> GetOrCreateProfileAsync(CancellationToken cancellationToken = default)
    {
        var profile = await _db.UserProfiles.OrderBy(p => p.CreatedAt).FirstOrDefaultAsync(cancellationToken);
        if (profile is not null)
        {
            return profile;
        }

        profile = new UserProfile();
        _db.UserProfiles.Add(profile);
        await _db.SaveChangesAsync(cancellationToken);
        return profile;
    }

    public async Task<UserProfile> UpdateProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var profile = await GetOrCreateProfileAsync(cancellationToken);
        if (request.DisplayName is not null) profile.DisplayName = request.DisplayName.Trim();
        if (request.TimeZone is not null) profile.TimeZone = request.TimeZone;
        if (request.PreferredLanguage is not null) profile.PreferredLanguage = request.PreferredLanguage;
        if (request.PreferredTone is not null) profile.PreferredTone = request.PreferredTone;
        if (request.DailyCheckInTime is not null) profile.DailyCheckInTime = request.DailyCheckInTime;
        profile.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return profile;
    }

    public async Task<string> BuildWeeklyReviewAsync(CancellationToken cancellationToken = default)
    {
        var since = DateTimeOffset.UtcNow.AddDays(-7);
        var progress = await _db.ProgressEntries.AsNoTracking().Where(p => p.CreatedAt >= since).ToListAsync(cancellationToken);
        var completed = await _db.Tasks.AsNoTracking()
            .Where(t => t.CompletedAt != null && t.CompletedAt >= since)
            .ToListAsync(cancellationToken);
        var goals = await _db.Goals.AsNoTracking().Include(g => g.Tasks)
            .Where(g => g.Status == GoalStatus.Active && g.ArchivedAt == null)
            .ToListAsync(cancellationToken);

        return $"Weekly review: {completed.Count} tasks completed, {progress.Count} progress updates. Active goals: {string.Join("; ", goals.Select(g => $"{g.Title} ({g.ProgressPercent}%)"))}.";
    }
}
