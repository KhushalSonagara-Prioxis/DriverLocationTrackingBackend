using DemoProject.Controllers;
using DLT.Service.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.RequestModel;

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
    
    [Authorize(Roles = "Admin")]
    [HttpGet("AdminDashBoardBarChart")]
    public async Task<IActionResult> AdminDashBoardBarChart([FromQuery]AdminDashBoardChartRequestModel requestModel)
    {
        var res = await _dashboardRepository.AdminDashBoardBarChart(requestModel);
        return Ok(res);
    }
}