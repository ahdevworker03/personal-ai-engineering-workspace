namespace PersonalAssistant.Domain.Entities;

public class UserProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string DisplayName { get; set; } = "User";
    public string TimeZone { get; set; } = "UTC";
    public string PreferredLanguage { get; set; } = "en";
    public string PreferredTone { get; set; } = "supportive and direct";
    public TimeOnly? DailyCheckInTime { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
