using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using PersonalAssistant.Application.Tasks;
using PersonalAssistant.Domain.Enums;

namespace PersonalAssistant.Api.Controllers;

[ApiController]
public sealed class TasksController : ControllerBase
{
    private readonly TaskService _tasks;
    private readonly IValidator<CreateTaskRequest> _createValidator;

    public TasksController(TaskService tasks, IValidator<CreateTaskRequest> createValidator)
    {
        _tasks = tasks;
        _createValidator = createValidator;
    }

    [HttpGet("api/goals/{goalId:guid}/tasks")]
    public async Task<IActionResult> ListByGoal(Guid goalId, [FromQuery] GoalTaskStatus? status, CancellationToken cancellationToken)
        => Ok(await _tasks.ListByGoalAsync(goalId, status, cancellationToken));

    [HttpPost("api/goals/{goalId:guid}/tasks")]
    public async Task<IActionResult> Create(Guid goalId, [FromBody] CreateTaskRequest request, CancellationToken cancellationToken)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);
        var task = await _tasks.CreateAsync(goalId, request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = task.Id }, task);
    }

    [HttpGet("api/tasks/{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var task = await _tasks.GetAsync(id, cancellationToken);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpPatch("api/tasks/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        var task = await _tasks.UpdateAsync(id, request, cancellationToken);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpPost("api/tasks/{id:guid}/progress")]
    public async Task<IActionResult> Progress(Guid id, [FromBody] RecordProgressRequest request, CancellationToken cancellationToken)
        => Ok(await _tasks.RecordProgressAsync(id, request, cancellationToken));

    [HttpPost("api/tasks/{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken)
    {
        var task = await _tasks.CompleteAsync(id, cancellationToken);
        return task is null ? NotFound() : Ok(task);
    }
}
