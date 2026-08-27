using Microsoft.AspNetCore.Mvc;
using PersonalAssistant.Application.Dashboard;

namespace PersonalAssistant.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public sealed class DashboardController : ControllerBase
{
    private readonly DashboardService _dashboard;

    public DashboardController(DashboardService dashboard)
    {
        _dashboard = dashboard;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
        => Ok(await _dashboard.GetAsync(cancellationToken));
}
