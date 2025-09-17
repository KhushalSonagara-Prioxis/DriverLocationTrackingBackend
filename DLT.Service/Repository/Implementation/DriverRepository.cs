using Common;
using DLT.Models.Models.DriverLocationTracking;
using DLT.Service.Repository.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Models.Models.CommonModel;
using Models.Models.SpDbContext;
using Models.RequestModel;
using Models.ResponsetModel;
using Newtonsoft.Json;
using Serilog;
using Service.RepositoryFactory;
using Service.UnitOfWork;

namespace DLT.Service.Repository.Implementation;

public class DriverRepository : IDriverRepository
{
    private readonly DriverLocationTrackingDbContext _context;
    private readonly DriverLocationTrackingSpContext _spContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DriverRepository(DriverLocationTrackingDbContext context, DriverLocationTrackingSpContext spContext, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _spContext = spContext;
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }

    #region UpdateDriverCurrectLocationAsync
    public async Task<bool> UpdateDriverCurrectLocationAsync(string tripSID, DriverCurrentLocationRequestModel driverCurrentLocation)
    {
        Log.Information("Updating driver current location for TripSID: {TripSID}", tripSID);

        try
        {
            var trip = await _unitOfWork.GetRepository<Trip>().SingleOrDefaultAsync(t => t.TripSid == tripSID); 
            if (trip == null)
            {
                Log.Warning("Trip not found for TripSID: {TripSID}", tripSID);
                throw new HttpStatusCodeException((int)StatusCode.NotFound, "Trip not found");
            }

            DriverCurrentLocation driverCurrectLocation = await _unitOfWork.GetRepository<DriverCurrentLocation>()
                .SingleOrDefaultAsync(t => t.TripId == trip.TripId);
           
            if (driverCurrectLocation == null)
            {
                Log.Warning("Driver current location not found for TripID: {TripID}", trip.TripId);
                throw new HttpStatusCodeException((int)StatusCode.NotFound, "Driver Currect Location not found");
            }

            driverCurrectLocation.Latitude = driverCurrentLocation.Latitude;
            driverCurrectLocation.Longitude = driverCurrentLocation.Longitude;
            driverCurrectLocation.LastUpdate = DateTime.Now;

            _unitOfWork.GetRepository<DriverCurrentLocation>().Update(driverCurrectLocation);
            await _unitOfWork.CommitAsync();

            Log.Information("Driver location updated successfully: Latitude={Latitude}, Longitude={Longitude}", 
                driverCurrectLocation.Latitude, driverCurrectLocation.Longitude);

            return true;
        }
        catch (HttpStatusCodeException ex)
        {
            Log.Warning(ex, "Known error occurred while updating driver location for TripSID: {TripSID}", tripSID);
            throw;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unexpected error occurred while updating driver location for TripSID: {TripSID}", tripSID);
            throw new HttpStatusCodeException((int)StatusCode.InternalServerError, ex);
        }
    }
    #endregion
    
    #region GetDriverCurrectLocationAsync
    public async Task<DriverCurrectLocationResponseModel> GetDriverCurrectLocationAsync(string tripSID)
    {
        Log.Information("Fetching driver current location for TripSID: {TripSID}", tripSID);

        try
        {
            string query = "sp_GetTripDetails {0}";
            object[] param = { tripSID };
            var res = await _spContext.ExecuteStoreProcedure(query, param);

            DriverCurrectLocationResponseModel tripDetail =
                JsonConvert.DeserializeObject<DriverCurrectLocationResponseModel>(res?.ToString() ?? "{}");

            if (tripDetail == null)
            {
                Log.Warning("No trip details found for TripSID: {TripSID}", tripSID);
                throw new HttpStatusCodeException((int)StatusCode.NotFound, "Trip not found");
            }

            Log.Information("Trip details retrieved successfully for TripSID: {TripSID}", tripSID);
            return tripDetail;
        }
        catch (HttpStatusCodeException ex)
        {
            Log.Warning(ex, "Known error while fetching trip details for TripSID: {TripSID}", tripSID);
            throw;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unexpected error while fetching trip details for TripSID: {TripSID}", tripSID);
            throw new HttpStatusCodeException((int)StatusCode.InternalServerError, ex);
        }
    }
    #endregion
    
    #region GetAllDrivers
    public async Task<List<DriverResponseModel>> GetAllDrivers()
    {
        Log.Information("Fetching all drivers");

        try
        {
            var drivers = await _context.Users.Where(d => d.Role == (int)StatusEnum.Driver).ToListAsync();

            if (drivers == null || !drivers.Any())
            {
                Log.Warning("No drivers found in database");
                throw new HttpStatusCodeException((int)StatusCode.NotFound, "Driver not found");
            }

            List<DriverResponseModel> res =
                JsonConvert.DeserializeObject<List<DriverResponseModel>>(JsonConvert.SerializeObject(drivers));

            if (res == null && !drivers.Any())
            {
                Log.Warning("Driver data deserialization failed");
                throw new HttpStatusCodeException((int)StatusCode.NotFound, "Driver not found");
            }

            Log.Information("Successfully retrieved {DriverCount} drivers", res.Count);
            return res;
        }
        catch (HttpStatusCodeException ex)
        {
            Log.Warning(ex, "Known error occurred while fetching all drivers");
            throw;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unexpected error occurred while fetching all drivers");
            throw new HttpStatusCodeException((int)StatusCode.InternalServerError, ex);
        }
    }
    #endregion
    
    #region GetAllTripsOfDriver
    public async Task<Page> GetAllTripsOfDrivers(Dictionary<string, object> parameters)
    {
        Log.Information("Fetching all trips for driver with parameters: {Parameters}", parameters);

        try
        {
            var xmlParams = Common.CommonHelper.DictionaryToXml(parameters, "Search");
            string userSID = _httpContextAccessor.HttpContext?.Items["UserSID"]?.ToString();

            string query = "sp_SearchTrips {0} , {1}";
            object[] param = { xmlParams, userSID };

            var res = await _spContext.ExecutreStoreProcedureResultList(query, param);

            if (res == null)
            {
                Log.Warning("No trips found for Driver UserSID: {UserSID}", userSID);
                throw new HttpStatusCodeException((int)StatusCode.BadRequest, "No results found");
            }

            Log.Information("Successfully retrieved trips for Driver UserSID: {UserSID}", userSID);
            return res;
        }
        catch (HttpStatusCodeException ex)
        {
            Log.Warning(ex, "Known error while fetching trips for driver");
            throw;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unexpected error while fetching trips for driver");
            throw new HttpStatusCodeException((int)StatusCode.InternalServerError, ex.Message);
        }
    }
    #endregion 
}
