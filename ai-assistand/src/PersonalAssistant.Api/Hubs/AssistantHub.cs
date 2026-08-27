using Microsoft.AspNetCore.SignalR;
using PersonalAssistant.Application.Abstractions;

namespace PersonalAssistant.Api.Hubs;

public sealed class AssistantHub : Hub
{
    public const string HubPath = "/hubs/assistant";
}

public sealed class SignalRNotificationPublisher : INotificationPublisher
{
    private readonly IHubContext<AssistantHub> _hub;
    private readonly Application.Abstractions.IAppDbContext _db;

    public SignalRNotificationPublisher(IHubContext<AssistantHub> hub, Application.Abstractions.IAppDbContext db)
    {
        _hub = hub;
        _db = db;
    }

    public async Task PublishAsync(string title, string message, Guid? reminderId = null, CancellationToken cancellationToken = default)
    {
        var notification = new Domain.Entities.AppNotification
        {
            Title = title,
            Message = message,
            ReminderId = reminderId
        };

        if (reminderId is null)
        {
            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync(cancellationToken);
        }

        await _hub.Clients.All.SendAsync("notification", new
        {
            id = notification.Id,
            title,
            message,
            reminderId,
            createdAt = DateTimeOffset.UtcNow
        }, cancellationToken);

        await _hub.Clients.All.SendAsync("dashboardChanged", new { at = DateTimeOffset.UtcNow }, cancellationToken);
    }
}
