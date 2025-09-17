using Common;
using DemoProject.Controllers;
using DLT.Models.Models.DriverLocationTracking;
using DLT.Service.Repository.Interface;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace DLT.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationController : BaseController
{
    private readonly ILocationRepository _locationRepository;

    public LocationController(ILocationRepository locationRepository)
    {
        _locationRepository = locationRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllLocation()
    {
        Log.Information("Fetching all locations");

        var res = await _locationRepository.GetAllLocation();
        Log.Information("Repository returned result: {@Result}", res);

        if (res == null)
        {
            Log.Warning("Location data is null");
            return NotFound();
        }

        if (!res.Any())
        {
            Log.Warning("No locations found");
            throw new HttpStatusCodeException((int)Common.StatusCode.BadRequest, "No results found");
        }

        Log.Information("Successfully fetched {LocationCount} locations", res.Count());
        return Ok(res);
    }
}