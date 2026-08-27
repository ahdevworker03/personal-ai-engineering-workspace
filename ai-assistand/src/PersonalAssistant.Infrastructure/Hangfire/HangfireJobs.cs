using Hangfire;
using PersonalAssistant.Application.Abstractions;
using PersonalAssistant.Application.Reminders;

namespace PersonalAssistant.Infrastructure.HangfireJobs;

public sealed class HangfireReminderScheduler : IReminderScheduler
{
    public string Schedule(Guid reminderId, DateTimeOffset scheduledAt)
    {
        var delay = scheduledAt - DateTimeOffset.UtcNow;
        if (delay < TimeSpan.Zero)
        {
            delay = TimeSpan.Zero;
        }

        return BackgroundJob.Schedule<ReminderDeliveryJob>(job => job.ExecuteAsync(reminderId, CancellationToken.None), delay);
    }

    public void Cancel(string jobId)
    {
        BackgroundJob.Delete(jobId);
    }
}

public sealed class ReminderDeliveryJob
{
    private readonly ReminderService _reminders;
    private readonly INotificationPublisher _publisher;

    public ReminderDeliveryJob(ReminderService reminders, INotificationPublisher publisher)
    {
        _reminders = reminders;
        _publisher = publisher;
    }

    public async Task ExecuteAsync(Guid reminderId, CancellationToken cancellationToken)
    {
        await _reminders.DeliverAsync(reminderId, cancellationToken);
        await _publisher.PublishAsync("Reminder due", $"Reminder {reminderId} is due.", reminderId, cancellationToken);
    }
}

public sealed class MorningCheckInJob
{
    private readonly INotificationPublisher _publisher;

    public MorningCheckInJob(INotificationPublisher publisher)
    {
        _publisher = publisher;
    }

    public Task ExecuteAsync(CancellationToken cancellationToken) =>
        _publisher.PublishAsync("Morning check-in", "What is the smallest useful action for today?", cancellationToken: cancellationToken);
}

public sealed class EveningReviewJob
{
    private readonly INotificationPublisher _publisher;

    public EveningReviewJob(INotificationPublisher publisher)
    {
        _publisher = publisher;
    }

    public Task ExecuteAsync(CancellationToken cancellationToken) =>
        _publisher.PublishAsync("Evening review", "Record what you finished and what slipped.", cancellationToken: cancellationToken);
}
