using Microsoft.AspNetCore.Mvc;
using PersonalAssistant.Application.Abstractions;
using PersonalAssistant.Application.Memory;

namespace PersonalAssistant.Api.Controllers;

[ApiController]
[Route("api/voice")]
public sealed class VoiceController : ControllerBase
{
    private readonly ISpeechToTextService _stt;
    private readonly ITextToSpeechService _tts;

    public VoiceController(ISpeechToTextService stt, ITextToSpeechService tts)
    {
        _stt = stt;
        _tts = tts;
    }

    [HttpPost("transcribe")]
    public async Task<IActionResult> Transcribe(IFormFile audio, CancellationToken cancellationToken)
    {
        await using var stream = audio.OpenReadStream();
        var result = await _stt.TranscribeAsync(stream, audio.ContentType, cancellationToken);
        return Ok(result);
    }

    [HttpPost("synthesize")]
    public async Task<IActionResult> Synthesize([FromBody] SynthesizeBody body, CancellationToken cancellationToken)
    {
        var bytes = await _tts.SynthesizeAsync(body.Text, body.VoiceId, cancellationToken);
        return File(bytes, "audio/mpeg", "speech.mp3");
    }
}

[ApiController]
[Route("api")]
public sealed class SettingsController : ControllerBase
{
    private readonly MemoryService _memory;

    public SettingsController(MemoryService memory)
    {
        _memory = memory;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
        => Ok(await _memory.GetOrCreateProfileAsync(cancellationToken));

    [HttpPatch("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
        => Ok(await _memory.UpdateProfileAsync(request, cancellationToken));

    [HttpGet("memories")]
    public async Task<IActionResult> ListMemories(CancellationToken cancellationToken)
        => Ok(await _memory.ListAsync(cancellationToken));

    [HttpPost("memories")]
    public async Task<IActionResult> UpsertMemory([FromBody] UpsertMemoryRequest request, CancellationToken cancellationToken)
        => Ok(await _memory.UpsertAsync(request, cancellationToken));

    [HttpGet("reviews/weekly")]
    public async Task<IActionResult> Weekly(CancellationToken cancellationToken)
        => Ok(new { summary = await _memory.BuildWeeklyReviewAsync(cancellationToken) });
}

public sealed record SynthesizeBody(string Text, string? VoiceId);
