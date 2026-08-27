namespace PersonalAssistant.Application.Abstractions;

public interface ISpeechToTextService
{
    Task<TranscriptionResult> TranscribeAsync(Stream audio, string contentType, CancellationToken cancellationToken = default);
}

public sealed class TranscriptionResult
{
    public required string Text { get; init; }
}

public interface ITextToSpeechService
{
    Task<byte[]> SynthesizeAsync(string text, string? voiceId = null, CancellationToken cancellationToken = default);
}

public interface INotificationPublisher
{
    Task PublishAsync(string title, string message, Guid? reminderId = null, CancellationToken cancellationToken = default);
}

public interface IReminderScheduler
{
    string Schedule(Guid reminderId, DateTimeOffset scheduledAt);
    void Cancel(string jobId);
}
