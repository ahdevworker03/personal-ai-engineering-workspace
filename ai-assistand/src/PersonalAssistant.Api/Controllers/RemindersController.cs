using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using PersonalAssistant.Application.Reminders;

namespace PersonalAssistant.Api.Controllers;

[ApiController]
[Route("api/reminders")]
public sealed class RemindersController : ControllerBase
{
    private readonly ReminderService _reminders;
    private readonly IValidator<CreateReminderRequest> _validator;

    public RemindersController(ReminderService reminders, IValidator<CreateReminderRequest> validator)
    {
        _reminders = reminders;
        _validator = validator;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] DateTimeOffset? start, [FromQuery] DateTimeOffset? end, CancellationToken cancellationToken)
        => Ok(await _reminders.ListAsync(start, end, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReminderRequest request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);
        return Ok(await _reminders.CreateAsync(request, cancellationToken));
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReminderRequest request, CancellationToken cancellationToken)
    {
        var reminder = await _reminders.UpdateAsync(id, request, cancellationToken);
        return reminder is null ? NotFound() : Ok(reminder);
    }

    [HttpPost("{id:guid}/snooze")]
    public async Task<IActionResult> Snooze(Guid id, [FromBody] SnoozeRequest request, CancellationToken cancellationToken)
    {
        var reminder = await _reminders.SnoozeAsync(id, request.NewScheduledAt, cancellationToken);
        return reminder is null ? NotFound() : Ok(reminder);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
        => await _reminders.CancelAsync(id, cancellationToken) ? NoContent() : NotFound();
}

public sealed record SnoozeRequest(DateTimeOffset NewScheduledAt);
