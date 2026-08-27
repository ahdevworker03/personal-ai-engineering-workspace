using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using PersonalAssistant.Application.Goals;

namespace PersonalAssistant.Api.Controllers;

[ApiController]
[Route("api/goals")]
public sealed class GoalsController : ControllerBase
{
    private readonly GoalService _goals;
    private readonly IValidator<CreateGoalRequest> _createValidator;

    public GoalsController(GoalService goals, IValidator<CreateGoalRequest> createValidator)
    {
        _goals = goals;
        _createValidator = createValidator;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
        => Ok(await _goals.ListAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var goal = await _goals.GetAsync(id, cancellationToken);
        return goal is null ? NotFound() : Ok(goal);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGoalRequest request, CancellationToken cancellationToken)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);
        var goal = await _goals.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = goal.Id }, goal);
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGoalRequest request, CancellationToken cancellationToken)
    {
        var goal = await _goals.UpdateAsync(id, request, cancellationToken);
        return goal is null ? NotFound() : Ok(goal);
    }

    [HttpPost("{id:guid}/archive")]
    public async Task<IActionResult> Archive(Guid id, CancellationToken cancellationToken)
        => await _goals.ArchiveAsync(id, cancellationToken) ? NoContent() : NotFound();
}
