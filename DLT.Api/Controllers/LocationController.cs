using Common;
using DemoProject.Controllers;
using DLT.Models.Models.DriverLocationTracking;
using DLT.Service.Repository.Interface;
using Microsoft.AspNetCore.Mvc;

namespace DLT.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
public class LocationController : BaseController
{
    private readonly ILocationRepository  _locationRepository;

    public LocationController(ILocationRepository locationRepository)
    {
        _locationRepository = locationRepository;
    }
    [HttpGet]
    public async Task<IActionResult> GetAllLocation()
    {
        var res = await _locationRepository.GetAllLocation();
        if (res == null)
        {
            return NotFound();
        }

        if (res.Count() == 0)
        {
            throw new HttpStatusCodeException((int)Common.StatusCode.BadRequest, "No results found");
        }
        return Ok(res);
    }
}