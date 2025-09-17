using Common;
using DemoProject.Controllers;
using DLT.Service.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Models.CommonModel;
using Models.RequestModel;
using Models.ResponsetModel;
using Newtonsoft.Json;

namespace DLT.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Driver")]
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

    [HttpGet("GetDrivers")]
    public async Task<IActionResult> GetDrivers()
    {
        var res = await _repository.GetAllDrivers();
        if (res == null)
        {
            return NotFound();
        }
        return Ok(res);
    }
    [HttpGet("GetAllTripsOfDriver")]
    public async Task<ActionResult> GetAllTrips([FromQuery] SearchRequestModel searchModel)
    {
        var parameters = FillParamesFromModel(searchModel);
        var list = await _repository.GetAllTripsOfDrivers(parameters);
        List<TripListResponseModel> response = JsonConvert.DeserializeObject<List<TripListResponseModel>>(list.Result?.ToString() ?? "[]") ?? [];
        if (response == null)
        {
            return BadRequest();
        }

        if (response.Count == 0)
        {
            throw new HttpStatusCodeException((int)Common.StatusCode.BadRequest, "No results found");
        }
        list.Result = response;
        if (response == null || !response.Any())
        {
            return NotFound();
        }

        return Ok(BindSearchResult(list, searchModel, "all trips of driver"));
    }
}