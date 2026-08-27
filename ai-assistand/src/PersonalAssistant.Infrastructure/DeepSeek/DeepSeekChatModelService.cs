using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using PersonalAssistant.Application.Abstractions;

namespace PersonalAssistant.Infrastructure.DeepSeek;

public sealed class DeepSeekOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.deepseek.com";
    public string Model { get; set; } = "deepseek-chat";
}

public sealed class DeepSeekChatModelService : IChatModelService
{
    private readonly HttpClient _http;
    private readonly DeepSeekOptions _options;

    public DeepSeekChatModelService(HttpClient http, IOptions<DeepSeekOptions> options)
    {
        _http = http;
        _options = options.Value;
    }

    public async Task<AssistantChatResult> GenerateAsync(AssistantChatRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return new AssistantChatResult
            {
                Content = "DeepSeek API key is not configured. Set DeepSeek__ApiKey in environment or appsettings, then ask again."
            };
        }

        var messages = request.Messages.Select(m =>
        {
            var dict = new Dictionary<string, object?>
            {
                ["role"] = m.Role,
                ["content"] = m.Content
            };
            if (!string.IsNullOrWhiteSpace(m.ToolCallId)) dict["tool_call_id"] = m.ToolCallId;
            if (!string.IsNullOrWhiteSpace(m.Name)) dict["name"] = m.Name;
            if (m.ToolCalls is { Count: > 0 })
            {
                dict["tool_calls"] = m.ToolCalls.Select(tc => new
                {
                    id = tc.Id,
                    type = "function",
                    function = new { name = tc.Name, arguments = tc.ArgumentsJson }
                }).ToList();
            }
            return dict;
        }).ToList();

        var tools = request.Tools.Select(t => new
        {
            type = "function",
            function = new
            {
                name = t.Name,
                description = t.Description,
                parameters = JsonSerializer.Deserialize<JsonElement>(t.ParametersJsonSchema)
            }
        }).ToList();

        var payload = new
        {
            model = _options.Model,
            messages,
            tools,
            tool_choice = "auto"
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_options.BaseUrl.TrimEnd('/')}/v1/chat/completions");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        httpRequest.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        using var response = await _http.SendAsync(httpRequest, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"DeepSeek error {(int)response.StatusCode}: {body}");
        }

        using var doc = JsonDocument.Parse(body);
        var message = doc.RootElement.GetProperty("choices")[0].GetProperty("message");
        var content = message.TryGetProperty("content", out var c) && c.ValueKind != JsonValueKind.Null ? c.GetString() : null;
        var toolCalls = new List<ToolCallDto>();

        if (message.TryGetProperty("tool_calls", out var calls) && calls.ValueKind == JsonValueKind.Array)
        {
            foreach (var call in calls.EnumerateArray())
            {
                var fn = call.GetProperty("function");
                toolCalls.Add(new ToolCallDto
                {
                    Id = call.GetProperty("id").GetString() ?? Guid.NewGuid().ToString("N"),
                    Name = fn.GetProperty("name").GetString() ?? string.Empty,
                    ArgumentsJson = fn.GetProperty("arguments").GetString() ?? "{}"
                });
            }
        }

        return new AssistantChatResult
        {
            Content = content,
            ToolCalls = toolCalls
        };
    }
}
