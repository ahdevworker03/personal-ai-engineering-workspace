using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PersonalAssistant.Application.Abstractions;

namespace PersonalAssistant.Infrastructure.FishAudio;

public sealed class FishAudioOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.fish.audio";
    public string VoiceId { get; set; } = string.Empty;
}

public sealed class FishAudioSpeechService : ISpeechToTextService, ITextToSpeechService
{
    private readonly HttpClient _http;
    private readonly FishAudioOptions _options;

    public FishAudioSpeechService(HttpClient http, IOptions<FishAudioOptions> options)
    {
        _http = http;
        _options = options.Value;
    }

    public async Task<TranscriptionResult> TranscribeAsync(Stream audio, string contentType, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();
        using var form = new MultipartFormDataContent();
        var streamContent = new StreamContent(audio);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrWhiteSpace(contentType) ? "audio/webm" : contentType);
        form.Add(streamContent, "audio", "recording.webm");

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{_options.BaseUrl.TrimEnd('/')}/v1/asr");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        request.Content = form;

        using var response = await _http.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Fish Audio ASR error {(int)response.StatusCode}: {body}");
        }

        using var doc = JsonDocument.Parse(body);
        var text = doc.RootElement.TryGetProperty("text", out var t) ? t.GetString() : body;
        return new TranscriptionResult { Text = text ?? string.Empty };
    }

    public async Task<byte[]> SynthesizeAsync(string text, string? voiceId = null, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();
        var payload = new Dictionary<string, object?>
        {
            ["text"] = text,
            ["format"] = "mp3"
        };
        var voice = string.IsNullOrWhiteSpace(voiceId) ? _options.VoiceId : voiceId;
        if (!string.IsNullOrWhiteSpace(voice))
        {
            payload["reference_id"] = voice;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{_options.BaseUrl.TrimEnd('/')}/v1/tts");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        using var response = await _http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Fish Audio TTS error {(int)response.StatusCode}: {body}");
        }

        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("Fish Audio API key is not configured. Set FishAudio__ApiKey.");
        }
    }
}
