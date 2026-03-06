using DemoProject.Controllers;
using DLT.Service.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DLT.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : BaseController
{
    private readonly IDashboardRepository _dashboardRepository;

    public DashboardController(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("AdminDashboard")]
    public async Task<IActionResult> AdminDashboard()
    {
        var res = await _dashboardRepository.AdminDashBoard();
        return Ok(res);
    }
}