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
public class TripController : BaseController
{
    private readonly ITripRepository _tripRepository;

    public TripController(ITripRepository tripRepository)
    {
        _tripRepository = tripRepository;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> GetAllTrips([FromQuery] SearchRequestModel searchModel)
    {
        Log.Information("Fetching all trips with search model: {@SearchModel}", searchModel);

        var parameters = FillParamesFromModel(searchModel);
        var list = await _tripRepository.GetAllTrips(parameters);
        List<TripListResponseModel> response = JsonConvert.DeserializeObject<List<TripListResponseModel>>(list.Result?.ToString() ?? "[]") ?? [];

        if (response == null)
        {
            Log.Warning("GetAllTrips returned null response");
            return BadRequest();
        }

        if (response.Count == 0)
        {
            Log.Warning("No results found for search: {@SearchModel}", searchModel);
            throw new HttpStatusCodeException((int)Common.StatusCode.BadRequest, "No results found");
        }

        list.Result = response;
        if (response == null || !response.Any())
        {
            Log.Warning("GetAllTrips result is empty after deserialization");
            return NotFound();
        }

        Log.Information("Successfully fetched {Count} trips", response.Count);
        return Ok(BindSearchResult(list, searchModel, "All Trips"));
    }

    [HttpPost("AddTripStatus/{tripSID}")]
    [Authorize(Roles = "Driver")]
    public async Task<ActionResult> AddTripStatus([FromRoute] string tripSID, TripUpdateStatusRequestModel tripUpdateStatusRequestModel)
    {
        Log.Information("Adding trip status for TripSID: {TripSID}", tripSID);

        var success = await _tripRepository.AddTripUpdate(tripSID, tripUpdateStatusRequestModel);
        if (!success)
        {
            Log.Error("Failed to add trip status for TripSID: {TripSID}", tripSID);
            return BadRequest();
        }

        Log.Information("Trip status added successfully for TripSID: {TripSID}", tripSID);
        return Ok(new { message = "Trip Update added successfully" });
    }

    [HttpGet("GetTripUpdateStatus/{tripSID}")]
    [Authorize]
    public async Task<ActionResult> GetTripUpdateStatus([FromRoute] string tripSID)
    {
        Log.Information("Fetching trip update status for TripSID: {TripSID}", tripSID);

        var response = await _tripRepository.GetAllTripUpdateStatus(tripSID);
        if (response == null)
        {
            Log.Warning("No trip update status found for TripSID: {TripSID}", tripSID);
            return BadRequest();
        }

        Log.Information("Successfully fetched trip update status for TripSID: {TripSID}", tripSID);
        return Ok(response);
    }

    [HttpPost("AddTrip")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> AddTrip([FromBody] TripRequestModel request)
    {
        Log.Information("Creating a new trip: {@Request}", request);

        var success = await _tripRepository.CreateTrip(request);
        if (!success)
        {
            Log.Error("Failed to create trip: {@Request}", request);
            return BadRequest();
        }

        Log.Information("Trip created successfully");
        return Ok(new { message = "Trip created successfully" });
    }

    [HttpPost("TripStart/{tripSID}")]
    [Authorize(Roles = "Driver")]
    public async Task<ActionResult> TripStart([FromRoute] string tripSID)
    {
        Log.Information("Starting trip with TripSID: {TripSID}", tripSID);

        var success = await _tripRepository.TripsStart(tripSID);
        if (!success)
        {
            Log.Error("Failed to start trip with TripSID: {TripSID}", tripSID);
            return BadRequest();
        }

        Log.Information("Trip started successfully for TripSID: {TripSID}", tripSID);
        return Ok(new { message = "Trip start successfully" });
    }

    [HttpPost("TripEnd/{tripSID}")]
    [Authorize(Roles = "Driver")]
    public async Task<ActionResult> TripEnd([FromRoute] string tripSID)
    {
        Log.Information("Ending trip with TripSID: {TripSID}", tripSID);

        var success = await _tripRepository.TripsEnd(tripSID);
        if (!success)
        {
            Log.Error("Failed to end trip with TripSID: {TripSID}", tripSID);
            return BadRequest();
        }

        Log.Information("Trip ended successfully for TripSID: {TripSID}", tripSID);
        return Ok(new { message = "Trip Ended successfully" });
    }

    [HttpDelete("DeleteTrip/{tripSID}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteTrip([FromRoute] string tripSID)
    {
        Log.Information("Deleting trip with TripSID: {TripSID}", tripSID);

        var success = await _tripRepository.DeleteTrip(tripSID);
        if (!success)
        {
            Log.Error("Failed to delete trip with TripSID: {TripSID}", tripSID);
            return BadRequest();
        }

        Log.Information("Trip deleted successfully with TripSID: {TripSID}", tripSID);
        return Ok(new { message = "Trip Deleted successfully" });
    }

    [HttpPost("UpdateTrip/{tripSID}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> UpdateTrip([FromRoute] string tripSID, [FromBody] TripRequestModel request)
    {
        Log.Information("Updating trip with TripSID: {TripSID}", tripSID);

        var success = await _tripRepository.UpdateTrip(tripSID, request);
        if (!success)
        {
            Log.Error("Failed to update trip with TripSID: {TripSID}", tripSID);
            return BadRequest();
        }

        Log.Information("Trip updated successfully for TripSID: {TripSID}", tripSID);
        return Ok(new { message = "Trip Updated successfully" });
    }
}
