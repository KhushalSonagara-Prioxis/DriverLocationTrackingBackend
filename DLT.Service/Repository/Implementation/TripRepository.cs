using Common;
using DLT.Models.Models.DriverLocationTracking;
using DLT.Service.Repository.Interface;
using Microsoft.AspNetCore.Http;
using Models.Models.CommonModel;
using Models.Models.SpDbContext;
using Models.RequestModel;
using Models.ResponsetModel;
using Newtonsoft.Json;
using Service.RepositoryFactory;
using Service.UnitOfWork;
using Serilog;

namespace DLT.Service.Repository.Implementation;

public class TripRepository : ITripRepository
{
    private readonly DriverLocationTrackingDbContext _context;
    private readonly DriverLocationTrackingSpContext _spContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public TripRepository(DriverLocationTrackingDbContext context, DriverLocationTrackingSpContext spContext, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _spContext = spContext;
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }
    #region GetAll trips
    public async Task<Page> GetAllTrips(Dictionary<string, object> parameters)
    {
        try
        {
            Log.Information("Starting GetAllTrips operation with parameters: {@Parameters}", parameters);
            
            var xmlParams = Common.CommonHelper.DictionaryToXml(parameters, "Search");
            string query = "sp_SearchTripsTest {0}";
            object[] param = { xmlParams};
            var res = await _spContext.ExecutreStoreProcedureResultList(query, param);
            if (res == null)
            {
                Log.Warning("GetAllTrips: No results found for parameters: {@Parameters}", parameters);
                throw new HttpStatusCodeException((int)StatusCode.BadRequest, "No results found");
            }

            Log.Information("GetAllTrips operation completed successfully.");
            return res;
        }
        catch (HttpStatusCodeException exception)
        {
            Log.Warning("GetAllTrips: HttpStatusCodeException occurred with status code {StatusCode} and message: {Message}", exception.StatusCode, exception.Message);
            throw;
        }
        catch (Exception exception)
        {
            Log.Error(exception, "GetAllTrips: Unexpected error occurred while retrieving trips with parameters: {@Parameters}", parameters);
            throw new HttpStatusCodeException((int)StatusCode.InternalServerError, exception.Message);
        }
    }
#endregion

    #region Create Trip

    public async Task<bool> CreateTrip(TripRequestModel model)
    {
        try
        {
            Log.Information("Starting CreateTrip operation for StartLocationSID: {StartLocationSID}, ToLocationSID: {ToLocationSID}, DriverSID: {DriverSID}", 
                model.StartLocationSID, model.ToLocationSID, model.DriverSID);
            
            var sLocation = await _unitOfWork.GetRepository<Location>()
                .SingleOrDefaultAsync(l => l.LocationSid == model.StartLocationSID);
            if (sLocation == null)
            {
                Log.Warning("CreateTrip: Start location not found for SID: {StartLocationSID}", model.StartLocationSID);
                throw new HttpStatusCodeException((int)StatusCode.BadRequest,"Start location not found");
            }
            var eLocation = await _unitOfWork.GetRepository<Location>()
                .SingleOrDefaultAsync(l => l.LocationSid == model.ToLocationSID);
            if (eLocation == null)
            {
                Log.Warning("CreateTrip: To location not found for SID: {ToLocationSID}", model.ToLocationSID);
                throw new HttpStatusCodeException((int)StatusCode.BadRequest, "To location not found");
            }

            var Driver = await _unitOfWork.GetRepository<User>()
                .SingleOrDefaultAsync(u => u.UserSid == model.DriverSID);
            if (Driver == null)
            {
                Log.Warning("CreateTrip: Driver not found for SID: {DriverSID}", model.DriverSID);
                throw new HttpStatusCodeException((int)StatusCode.BadRequest, "Driver not found");
            }
            string userSID = _httpContextAccessor.HttpContext?.Items["UserSID"]?.ToString();
            var admin = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(u => u.UserSid == userSID);
            Trip t = new Trip();
            t.TripSid = "TRI-" + Guid.NewGuid().ToString();
            t.StartLatitude = model.StartLatitude;
            t.StartLongitude = model.StartLongitude;
            t.StartLocation = sLocation.LocationId;
            t.ToLatitude = model.ToLatitude;
            t.ToLongitude = model.ToLongitude;
            t.ToLocation = eLocation.LocationId;
            t.DriverId = Driver.UserId;
            t.CreatedBy = admin.UserId;
            t.CreatedDate = DateTime.Now;
            t.LastModifiedBy = admin.UserId;
            t.LastModifiedDate = DateTime.Now;
            t.TripStatus = (int)StatusEnum.Pending;
            t.Status = (int)StatusEnum.Acitive;
            await _unitOfWork.GetRepository<Trip>().InsertAsync(t);
            await _unitOfWork.CommitAsync();
            
            Log.Information("CreateTrip operation completed successfully. Created trip with SID: {TripSID} for driver: {DriverSID}", 
                t.TripSid, model.DriverSID);
            return true;
        }
        catch (HttpStatusCodeException exception)
        {
            Log.Warning("CreateTrip: HttpStatusCodeException occurred with status code {StatusCode} and message: {Message}", exception.StatusCode, exception.Message);
            throw;
        }
        catch (Exception exception)
        {
            Log.Error(exception, "CreateTrip: Unexpected error occurred while creating trip for driver: {DriverSID}", model.DriverSID);
            throw new HttpStatusCodeException((int)StatusCode.InternalServerError, exception.Message);
        }
    }

#endregion
    
    #region AddTripUpdate
    public async Task<bool> AddTripUpdate(string TripSID, TripUpdateStatusRequestModel request)
    {
        try
        {
            Log.Information("Starting AddTripUpdate operation for TripSID: {TripSID} with status: {TripUpdateStatus}", TripSID, request.TripUpdateStatus);
            
            var trip = await _unitOfWork.GetRepository<Trip>().SingleOrDefaultAsync(t => t.TripSid == TripSID && t.TripStatus == (int)StatusEnum.InProgress);
            if (trip == null)
            {
                Log.Warning("AddTripUpdate: No in-progress trip found for SID: {TripSID}", TripSID);
                throw new HttpStatusCodeException((int)StatusCode.NotFound, "No results found");
            }

            TripUpdate tripUpdate = new TripUpdate();
            tripUpdate.DriverId = trip.DriverId ?? 0;
            tripUpdate.TripId = trip.TripId;
            tripUpdate.TripUpdatedLatitude = request.TripUpdatedLatitude;
            tripUpdate.TripUpdatedLongitude = request.TripUpdatedLongitude;
            tripUpdate.TripUpdatesSid = "TUS" + Guid.NewGuid().ToString();
            if (!(request.TripUpdateStatus >= 9 && request.TripUpdateStatus <= 12))
            {
                Log.Warning("AddTripUpdate: Invalid status provided: {TripUpdateStatus} for TripSID: {TripSID}", request.TripUpdateStatus, TripSID);
                throw new HttpStatusCodeException((int)StatusCode.BadRequest, "Invalid status");
            }

            tripUpdate.TripUpdatesStatus = request.TripUpdateStatus;
            tripUpdate.Note = request.Note;
            tripUpdate.TimeStamp = DateTime.Now;
            await _unitOfWork.GetRepository<TripUpdate>().InsertAsync(tripUpdate);
            await _unitOfWork.CommitAsync();
            
            Log.Information("AddTripUpdate operation completed successfully for TripSID: {TripSID} with update SID: {TripUpdateSid}", 
                TripSID, tripUpdate.TripUpdatesSid);
            return true;
        }
        catch (HttpStatusCodeException exception)
        {
            Log.Warning("AddTripUpdate: HttpStatusCodeException occurred for TripSID: {TripSID} with status code {StatusCode} and message: {Message}", 
                TripSID, exception.StatusCode, exception.Message);
            throw;
        }
        catch (Exception exception)
        {
            Log.Error(exception, "AddTripUpdate: Unexpected error occurred while adding trip update for TripSID: {TripSID}", TripSID);
            throw new HttpStatusCodeException((int)StatusCode.InternalServerError, exception.Message);
        }
    }
    #endregion
    
    #region TripStart
    public async Task<bool> TripsStart(string tripSID)
    {
        try
        {
            Log.Information("Starting TripsStart operation for TripSID: {TripSID}", tripSID);
            
            var trip = await _unitOfWork.GetRepository<Trip>().SingleOrDefaultAsync(t => t.TripSid == tripSID);
            if (trip == null)
            {
                Log.Warning("TripsStart: Trip not found for SID: {TripSID}", tripSID);
                throw new HttpStatusCodeException((int)StatusCode.NotFound, "No results found");
            }
            trip.TripStatus = (int)StatusEnum.InProgress;
            trip.LastModifiedDate =  DateTime.Now;
            _unitOfWork.GetRepository<Trip>().Update(trip);
            
            await _unitOfWork.CommitAsyncWithTransaction();
            TripUpdate tripUpdate = new TripUpdate();
            tripUpdate.TripUpdatesSid = "TUS" + Guid.NewGuid().ToString();
            tripUpdate.DriverId = trip.DriverId ?? 0;
            tripUpdate.TripId = trip.TripId;
            tripUpdate.TripUpdatesStatus = (int)StatusEnum.Start;
            tripUpdate.Note = "Stated trip";
            tripUpdate.TimeStamp = DateTime.Now;
            tripUpdate.TripUpdatedLatitude = trip.StartLatitude;
            tripUpdate.TripUpdatedLongitude = trip.StartLongitude;
            await _unitOfWork.GetRepository<TripUpdate>().InsertAsync(tripUpdate);
            await _unitOfWork.CommitAsyncWithTransaction();
            DriverCurrentLocation driverCurrentLocation = new DriverCurrentLocation();
            driverCurrentLocation.DriverCurrentLocationSid = "DCL-"+Guid.NewGuid().ToString();
            driverCurrentLocation.TripId = trip.TripId;
            driverCurrentLocation.Latitude = trip.StartLatitude;
            driverCurrentLocation.Longitude = trip.StartLongitude;
            driverCurrentLocation.LastUpdate =  DateTime.Now;
            await _unitOfWork.GetRepository<DriverCurrentLocation>().InsertAsync(driverCurrentLocation);
            await _unitOfWork.CommitAsyncWithTransaction();
            if (_unitOfWork.dbContextTransaction != null)
            {
                await _unitOfWork.dbContextTransaction.CommitAsync();
            }

            Log.Information("TripsStart operation completed successfully for TripSID: {TripSID}", tripSID);
            return true;
        }
        catch (HttpStatusCodeException exception)
        {
            if(_unitOfWork.dbContextTransaction != null)
            {
                await _unitOfWork.dbContextTransaction.RollbackAsync();
            }
            Log.Warning("TripsStart: HttpStatusCodeException occurred for TripSID: {TripSID} with status code {StatusCode} and message: {Message}", 
                tripSID, exception.StatusCode, exception.Message);
            throw;
        }
        catch (Exception exception)
        {
            if(_unitOfWork.dbContextTransaction != null)
            {
                await _unitOfWork.dbContextTransaction.RollbackAsync();
            }
            Log.Error(exception, "TripsStart: Unexpected error occurred while starting trip for TripSID: {TripSID}", tripSID);
            throw new HttpStatusCodeException((int)StatusCode.InternalServerError, exception.Message);
        }
    }
    #endregion
    
    #region TripEnd
    public async Task<bool> TripsEnd(string tripSID)
    {
        try
        {
            Log.Information("Starting TripsEnd operation for TripSID: {TripSID}", tripSID);
            
            var trip = await _unitOfWork.GetRepository<Trip>().SingleOrDefaultAsync(t => t.TripSid == tripSID);
            if (trip == null)
            {
                Log.Warning("TripsEnd: Trip not found for SID: {TripSID}", tripSID);
                throw new HttpStatusCodeException((int)StatusCode.NotFound, "No results found");
            }

            var driverCurrentLocation = await _unitOfWork.GetRepository<DriverCurrentLocation>()
                .SingleOrDefaultAsync(t => t.TripId == trip.TripId);
            if (driverCurrentLocation == null)
            {
                Log.Warning("TripsEnd: Driver Current Location not found for TripSID: {TripSID}, TripId: {TripId}", tripSID, trip.TripId);
                throw new HttpStatusCodeException((int)StatusCode.NotFound, "Driver Current Location not found");
            }
            trip.TripStatus = (int)StatusEnum.Completed;
            trip.LastModifiedDate =  DateTime.Now;
            _unitOfWork.GetRepository<Trip>().Update(trip);
            
            await _unitOfWork.CommitAsyncWithTransaction();
            TripUpdate tripUpdate = new TripUpdate();
            tripUpdate.TripUpdatesSid = "TUS" + Guid.NewGuid().ToString();
            tripUpdate.DriverId = trip.DriverId ?? 0;
            tripUpdate.TripId = trip.TripId;
            tripUpdate.TripUpdatesStatus = (int)StatusEnum.End;
            tripUpdate.Note = "Trip is Ended trip";
            tripUpdate.TimeStamp = DateTime.Now;
            tripUpdate.TripUpdatedLatitude = driverCurrentLocation.Latitude;
            tripUpdate.TripUpdatedLongitude = driverCurrentLocation.Longitude;
            await _unitOfWork.GetRepository<TripUpdate>().InsertAsync(tripUpdate);
            await _unitOfWork.CommitAsyncWithTransaction();
            if (_unitOfWork.dbContextTransaction != null)
            {
                await _unitOfWork.dbContextTransaction.CommitAsync();
            }

            Log.Information("TripsEnd operation completed successfully for TripSID: {TripSID}", tripSID);
            return true;
        }
        catch (HttpStatusCodeException exception)
        {
            if(_unitOfWork.dbContextTransaction != null)
            {
                await _unitOfWork.dbContextTransaction.RollbackAsync();
            }
            Log.Warning("TripsEnd: HttpStatusCodeException occurred for TripSID: {TripSID} with status code {StatusCode} and message: {Message}", 
                tripSID, exception.StatusCode, exception.Message);
            throw;
        }
        catch (Exception exception)
        {
            if(_unitOfWork.dbContextTransaction != null)
            {
                await _unitOfWork.dbContextTransaction.RollbackAsync();
            }
            Log.Error(exception, "TripsEnd: Unexpected error occurred while ending trip for TripSID: {TripSID}", tripSID);
            throw new HttpStatusCodeException((int)StatusCode.InternalServerError, exception.Message);
        }
    }
    #endregion
    
    #region GetAllTripUpdateStatus
    public async Task<List<TripUpdateResponseModel>> GetAllTripUpdateStatus(string tripSID)
    {
        try
        {
            Log.Information("Starting GetAllTripUpdateStatus operation for TripSID: {TripSID}", tripSID);
            
            string query = "sp_GetTripUpdates {0}";
            object[] param = { tripSID };
            var res = await _spContext.ExecuteStoreProcedure(query, param);
            List<TripUpdateResponseModel> tripUpdateResponseModels = JsonConvert.DeserializeObject<List<TripUpdateResponseModel>>(res?.ToString() ?? "[]");
            if (res == null)
            {
                Log.Warning("GetAllTripUpdateStatus: No results found for TripSID: {TripSID}", tripSID);
                throw new HttpStatusCodeException((int)StatusCode.NotFound, "No results found");
            }
            
            Log.Information("GetAllTripUpdateStatus operation completed successfully for TripSID: {TripSID}. Retrieved {UpdateCount} trip updates", 
                tripSID, tripUpdateResponseModels.Count);
            return tripUpdateResponseModels;
        }
        catch (HttpStatusCodeException exception)
        {
            Log.Warning("GetAllTripUpdateStatus: HttpStatusCodeException occurred for TripSID: {TripSID} with status code {StatusCode} and message: {Message}", 
                tripSID, exception.StatusCode, exception.Message);
            throw;
        }
        catch (Exception exception)
        {
            Log.Error(exception, "GetAllTripUpdateStatus: Unexpected error occurred while retrieving trip updates for TripSID: {TripSID}", tripSID);
            throw new HttpStatusCodeException((int)StatusCode.InternalServerError, exception.Message);
        }
    }
    #endregion

    #region DeteleteTrip

    public async Task<bool> DeleteTrip(string tripSID)
    {
        try
        {
            Log.Information("Starting DeleteTrip operation for TripSID: {TripSID}", tripSID);
            
            var trip = await _unitOfWork.GetRepository<Trip>()
                .SingleOrDefaultAsync(t => t.TripSid == tripSID);
            if (trip == null)
            {
                Log.Warning("DeleteTrip: Trip not found for SID: {TripSID}", tripSID);
                throw new HttpStatusCodeException((int)StatusCode.NotFound, "No results found");
            }

            if (trip.TripStatus != (int)StatusEnum.Pending)
            {
                Log.Warning("DeleteTrip: Cannot delete trip with status {TripStatus} for TripSID: {TripSID}", trip.TripStatus, tripSID);
                throw new HttpStatusCodeException((int)StatusCode.NotFound, "Can not Delete the Trip");
            }
            
            trip.Status = (int)StatusEnum.Delete;
            _unitOfWork.GetRepository<Trip>().Update(trip);
            await _unitOfWork.CommitAsync();
            
            Log.Information("DeleteTrip operation completed successfully for TripSID: {TripSID}", tripSID);
            return true;
        }
        catch (HttpStatusCodeException exception)
        {
            Log.Warning("DeleteTrip: HttpStatusCodeException occurred for TripSID: {TripSID} with status code {StatusCode} and message: {Message}", 
                tripSID, exception.StatusCode, exception.Message);
            throw;
        }
        catch (Exception exception)
        {
            Log.Error(exception, "DeleteTrip: Unexpected error occurred while deleting trip for TripSID: {TripSID}", tripSID);
            throw new HttpStatusCodeException((int)StatusCode.InternalServerError, exception.Message);
        }
    }

    #endregion

    #region UpdateTrip

    public async Task<bool> UpdateTrip(string tripSID, TripRequestModel model)
    {
        try
        {
            Log.Information("Starting UpdateTrip operation for TripSID: {TripSID} with StartLocationSID: {StartLocationSID}, ToLocationSID: {ToLocationSID}, DriverSID: {DriverSID}", 
                tripSID, model.StartLocationSID, model.ToLocationSID, model.DriverSID);
            
            var t = await _unitOfWork.GetRepository<Trip>().SingleOrDefaultAsync(t => t.TripSid == tripSID);
            if (t == null)
            {
                Log.Warning("UpdateTrip: Trip not found for SID: {TripSID}", tripSID);
                throw new HttpStatusCodeException((int)StatusCode.NotFound, "No results found");
            }
            if (t.TripStatus != (int)StatusEnum.Pending)
            {
                Log.Warning("UpdateTrip: Cannot update trip with status {TripStatus} for TripSID: {TripSID}", t.TripStatus, tripSID);
                throw new HttpStatusCodeException((int)StatusCode.BadRequest, "You cannot Update The Trip");
            }
            var sLocation = await _unitOfWork.GetRepository<Location>()
                .SingleOrDefaultAsync(l => l.LocationSid == model.StartLocationSID);
            if (sLocation == null)
            {
                Log.Warning("UpdateTrip: Start location not found for SID: {StartLocationSID} while updating TripSID: {TripSID}", model.StartLocationSID, tripSID);
                throw new HttpStatusCodeException((int)StatusCode.BadRequest,"Start location not found");
            }
            var eLocation = await _unitOfWork.GetRepository<Location>()
                .SingleOrDefaultAsync(l => l.LocationSid == model.ToLocationSID);
            if (eLocation == null)
            {
                Log.Warning("UpdateTrip: To location not found for SID: {ToLocationSID} while updating TripSID: {TripSID}", model.ToLocationSID, tripSID);
                throw new HttpStatusCodeException((int)StatusCode.BadRequest, "To location not found");
            }

            var Driver = await _unitOfWork.GetRepository<User>()
                .SingleOrDefaultAsync(u => u.UserSid == model.DriverSID);
            if (Driver == null)
            { 
                Log.Warning("UpdateTrip: Driver not found for SID: {DriverSID} while updating TripSID: {TripSID}", model.DriverSID, tripSID);
                throw new HttpStatusCodeException((int)StatusCode.BadRequest, "Driver not found");
            }
            string userSID = _httpContextAccessor.HttpContext?.Items["UserSID"]?.ToString();
            var admin = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(u => u.UserSid == userSID);
            t.StartLatitude = model.StartLatitude;
            t.StartLongitude = model.StartLongitude;
            t.ToLatitude = model.ToLatitude;
            t.ToLongitude = model.ToLongitude;
            t.StartLocation = sLocation.LocationId;
            t.ToLocation = eLocation.LocationId;
            t.DriverId = Driver.UserId;
            t.LastModifiedBy = admin.UserId;
            t.LastModifiedDate = DateTime.Now;
            _unitOfWork.GetRepository<Trip>().Update(t);
            await _unitOfWork.CommitAsync();
            
            Log.Information("UpdateTrip operation completed successfully for TripSID: {TripSID}", tripSID);
            return true;
        }
        catch (HttpStatusCodeException exception)
        {
            Log.Warning("UpdateTrip: HttpStatusCodeException occurred for TripSID: {TripSID} with status code {StatusCode} and message: {Message}", 
                tripSID, exception.StatusCode, exception.Message);
            throw;
        }
        catch (Exception exception)
        {
            Log.Error(exception, "UpdateTrip: Unexpected error occurred while updating trip for TripSID: {TripSID}", tripSID);
            throw new HttpStatusCodeException((int)StatusCode.InternalServerError, exception.Message);
        }
    }

    #endregion
    
}