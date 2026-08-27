using Microsoft.AspNetCore.Mvc;
using PersonalAssistant.Application.Assistant;

namespace PersonalAssistant.Api.Controllers;

[ApiController]
[Route("api/conversations")]
public sealed class ConversationsController : ControllerBase
{
    private readonly AssistantOrchestrator _assistant;

    public ConversationsController(AssistantOrchestrator assistant)
    {
        _assistant = assistant;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateConversationBody? body, CancellationToken cancellationToken)
        => Ok(await _assistant.CreateConversationAsync(body?.Title, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var conversation = await _assistant.GetConversationAsync(id, cancellationToken);
        return conversation is null ? NotFound() : Ok(conversation);
    }

    [HttpGet("{id:guid}/messages")]
    public async Task<IActionResult> Messages(Guid id, CancellationToken cancellationToken)
        => Ok(await _assistant.GetMessagesAsync(id, cancellationToken));

    [HttpPost("{id:guid}/messages")]
    public async Task<IActionResult> Send(Guid id, [FromBody] SendMessageBody body, CancellationToken cancellationToken)
        => Ok(await _assistant.SendMessageAsync(id, body.Content, cancellationToken));
}

public sealed record CreateConversationBody(string? Title);
public sealed record SendMessageBody(string Content);
