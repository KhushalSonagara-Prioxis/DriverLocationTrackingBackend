using DemoProject.Controllers;
using DLT.Service.Repository.Interface;
using Microsoft.AspNetCore.Mvc;
using Models.RequestModel;

namespace DLT.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
public class DriverController : BaseController
{
    private readonly IDriverRepository  _repository;

    public DriverController(IDriverRepository repository)
    {
        _repository = repository;
    }
    [HttpPost("UpdateDriverCurrentLocation/{tripSId}")]
    public async Task<IActionResult> UpdateDriverCurrentLocation([FromBody] DriverCurrentLocationRequestModel driverCurrentLocationRequestModel, [FromRoute] string tripSId)
    {
        var success = await _repository.UpdateDriverCurrectLocationAsync(tripSId, driverCurrentLocationRequestModel);
        if (!success)
        {
            return BadRequest();
        }
        return NoContent();
    }

    [HttpGet("GetCurrentLocation/{tripSId}")]
    public async Task<IActionResult> GetTripDetails([FromRoute]string tripSId)
    {
        
        var details = await _repository.GetDriverCurrectLocationAsync(tripSId);
        if (details == null)
        {
            return NotFound();
        }
        return Ok(details);
    }
}