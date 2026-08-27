namespace PersonalAssistant.Application.Abstractions;

public interface IChatModelService
{
    Task<AssistantChatResult> GenerateAsync(AssistantChatRequest request, CancellationToken cancellationToken = default);
}

public sealed class AssistantChatRequest
{
    public required IReadOnlyList<ChatMessageDto> Messages { get; init; }
    public required IReadOnlyList<ToolDefinitionDto> Tools { get; init; }
    public string? SystemPrompt { get; init; }
}

public sealed class ChatMessageDto
{
    public required string Role { get; init; }
    public required string Content { get; init; }
    public string? ToolCallId { get; init; }
    public string? Name { get; init; }
    public IReadOnlyList<ToolCallDto>? ToolCalls { get; init; }
}

public sealed class ToolDefinitionDto
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string ParametersJsonSchema { get; init; }
}

public sealed class ToolCallDto
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string ArgumentsJson { get; init; }
}

public sealed class AssistantChatResult
{
    public string? Content { get; init; }
    public IReadOnlyList<ToolCallDto> ToolCalls { get; init; } = Array.Empty<ToolCallDto>();
    public bool RequiresToolExecution => ToolCalls.Count > 0;
}
