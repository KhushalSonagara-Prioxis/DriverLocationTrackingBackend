using Common;
using DemoProject.Controllers;
using DLT.Service.Repository.Interface;
using Microsoft.AspNetCore.Mvc;
using Models.Models.CommonModel;
using Models.RequestModel;
using Models.ResponsetModel;
using Newtonsoft.Json;

namespace DLT.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
public class TripController : BaseController
{
    private readonly ITripRepository _tripRepository;

    public TripController(ITripRepository tripRepository)
    {
        _tripRepository = tripRepository;
    }
    [HttpGet]
    public async Task<ActionResult> GetAllTrips([FromQuery] SearchRequestModel searchModel)
    {
        var parameters = FillParamesFromModel(searchModel);
        var list = await _tripRepository.GetAllTrips(parameters);
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

        return Ok(BindSearchResult(list, searchModel, "All Trips"));
    }

    [HttpPost("AddTripStatus/{tripSID}")]
    public async Task<ActionResult> AddTripStatus([FromRoute]string tripSID,TripUpdateStatusRequestModel  tripUpdateStatusRequestModel)
    {
        var success = await _tripRepository.AddTripUpdate(tripSID, tripUpdateStatusRequestModel);
        if (!success)
        {
            return BadRequest();
        }
        return Ok(new { message = "Trip Update added successfully" });
    }

    [HttpGet("GetTripUpdateStatus/{tripSID}")]
    public async Task<ActionResult> GetTripUpdateStatus([FromRoute] string tripSID)
    {
        var response = await _tripRepository.GetAllTripUpdateStatus(tripSID);
        if (response == null)
        {
            return BadRequest();
        }
        return Ok(response);
    }

    [HttpPost("AddTrip")]
    public async Task<ActionResult> AddTrip([FromBody] TripRequestModel request)
    {
        var success = await _tripRepository.CreateTrip(request);
        if (!success)
        {
            return BadRequest();
        }
        return Ok(new { message = "Trip created successfully" });
    }

    [HttpPost("TripStart/{tripSID}")]
    public async Task<ActionResult> TripStart([FromRoute] string tripSID)
    {
        var success = await _tripRepository.TripsStart(tripSID);
        if (!success)
        {
            return BadRequest();
        }
        return Ok(new { message = "Trip start successfully" });
    }
    
    [HttpPost("TripEnd/{tripSID}")]
    public async Task<ActionResult> TripEnd([FromRoute] string tripSID)
    {
        var success = await _tripRepository.TripsEnd(tripSID);
        if (!success)
        {
            return BadRequest();
        }
        return Ok(new { message = "Trip Ended successfully" });
    }

    [HttpDelete("DeleteTrip/{tripSID}")]
    public async Task<ActionResult> DeleteTrip([FromRoute] string tripSID)
    {
        var success = await _tripRepository.DeleteTrip(tripSID);
        if (!success)
        {
            return BadRequest();
        }
        return Ok(new { message = "Trip Deleted successfully" });
    }
}