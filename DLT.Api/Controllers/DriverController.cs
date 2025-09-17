using Common;
using DemoProject.Controllers;
using DLT.Service.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Models.CommonModel;
using Models.RequestModel;
using Models.ResponsetModel;
using Newtonsoft.Json;
using Serilog; 
namespace DLT.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Driver")]
public class DriverController : BaseController
{
    private readonly IDriverRepository _repository;

    public DriverController(IDriverRepository repository)
    {
        _repository = repository;
    }

    [HttpPost("UpdateDriverCurrentLocation/{tripSId}")]
    public async Task<IActionResult> UpdateDriverCurrentLocation([FromBody] DriverCurrentLocationRequestModel driverCurrentLocationRequestModel, [FromRoute] string tripSId)
    {
        Log.Information("Updating driver location for TripId: {TripId} with request {@Request}", tripSId, driverCurrentLocationRequestModel);

        var success = await _repository.UpdateDriverCurrectLocationAsync(tripSId, driverCurrentLocationRequestModel);
        if (!success)
        {
            Log.Warning("Failed to update driver location for TripId: {TripId}", tripSId);
            return BadRequest();
        }

        Log.Information("Successfully updated driver location for TripId: {TripId}", tripSId);
        return NoContent();
    }

    [HttpGet("GetCurrentLocation/{tripSId}")]
    public async Task<IActionResult> GetTripDetails([FromRoute] string tripSId)
    {
        Log.Information("Fetching current location for TripId: {TripId}", tripSId);

        var details = await _repository.GetDriverCurrectLocationAsync(tripSId);
        if (details == null)
        {
            Log.Warning("No driver location found for TripId: {TripId}", tripSId);
            return NotFound();
        }

        Log.Information("Successfully fetched driver location for TripId: {TripId}", tripSId);
        return Ok(details);
    }

    [HttpGet("GetDrivers")]
    public async Task<IActionResult> GetDrivers()
    {
        Log.Information("Fetching all drivers");

        var res = await _repository.GetAllDrivers();
        if (res == null)
        {
            Log.Warning("No drivers found");
            return NotFound();
        }

        Log.Information("Successfully fetched {DriverCount} drivers", res.Count());
        return Ok(res);
    }

    [HttpGet("GetAllTripsOfDriver")]
    public async Task<ActionResult> GetAllTrips([FromQuery] SearchRequestModel searchModel)
    {
        Log.Information("Fetching all trips for driver with search filter {@SearchModel}", searchModel);

        var parameters = FillParamesFromModel(searchModel);
        var list = await _repository.GetAllTripsOfDrivers(parameters);
        List<TripListResponseModel> response = JsonConvert.DeserializeObject<List<TripListResponseModel>>(list.Result?.ToString() ?? "[]") ?? [];

        if (response == null)
        {
            Log.Error("Trip response deserialization failed for searchModel {@SearchModel}", searchModel);
            return BadRequest();
        }

        if (response.Count == 0)
        {
            Log.Warning("No trips found for driver with search filter {@SearchModel}", searchModel);
            throw new HttpStatusCodeException((int)Common.StatusCode.BadRequest, "No results found");
        }

        list.Result = response;

        Log.Information("Fetched {TripCount} trips for driver", response.Count);
        return Ok(BindSearchResult(list, searchModel, "all trips of driver"));
    }
}
