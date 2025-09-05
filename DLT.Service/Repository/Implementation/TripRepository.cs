using Common;
using DLT.Models.Models.DriverLocationTracking;
using DLT.Service.Repository.Interface;
using Models.Models.CommonModel;
using Models.Models.SpDbContext;
using Models.RequestModel;
using Models.ResponsetModel;
using Newtonsoft.Json;
using Service.RepositoryFactory;
using Service.UnitOfWork;

namespace DLT.Service.Repository.Implementation;

public class TripRepository : ITripRepository
{
    private readonly DriverLocationTrackingDbContext _context;
    private readonly DriverLocationTrackingSpContext _spContext;
    private readonly IUnitOfWork _unitOfWork;
    public TripRepository(DriverLocationTrackingDbContext context, DriverLocationTrackingSpContext spContext, IUnitOfWork unitOfWork)
    {
        _context = context;
        _spContext = spContext;
        _unitOfWork = unitOfWork;
    }
    #region GetAll trips
    public async Task<Page> GetAllTrips(Dictionary<string, object> parameters)
    {
        try
        {
            var xmlParams = Common.CommonHelper.DictionaryToXml(parameters, "Search");
            string query = "sp_SearchTripsTest {0}";
            object[] param = { xmlParams};
            var res = await _spContext.ExecutreStoreProcedureResultList(query, param);
            if (res == null)
            {
                throw new HttpStatusCodeException((int)StatusCode.BadRequest, "No results found");
            }

            return res;
        }
        catch (HttpStatusCodeException exception)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new HttpStatusCodeException((int)StatusCode.InternalServerError, exception.Message);
        }
    }
#endregion

    #region Create Trip

    public async Task<bool> CreateTrip(TripRequestModel model)
    {
        try
        {
            var sLocation = await _unitOfWork.GetRepository<Location>()
                .SingleOrDefaultAsync(l => l.LocationSid == model.StartLocationSID);
            if (sLocation == null)
            {
                throw new HttpStatusCodeException((int)StatusCode.BadRequest,"Start location not found");
            }
            var eLocation = await _unitOfWork.GetRepository<Location>()
                .SingleOrDefaultAsync(l => l.LocationSid == model.ToLocationSID);
            if (eLocation == null)
            {
                throw new HttpStatusCodeException((int)StatusCode.BadRequest, "To location not found");
            }

            var Driver = await _unitOfWork.GetRepository<User>()
                .SingleOrDefaultAsync(u => u.UserSid == model.DriverSID);
            if (Driver == null)
            {
                throw new HttpStatusCodeException((int)StatusCode.BadRequest, "Driver not found");
            }
            var admin = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(u => u.UserSid == model.UserSID);
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
            return true;
        }
        catch (HttpStatusCodeException exception)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new HttpStatusCodeException((int)StatusCode.InternalServerError, exception.Message);
        }
    }

#endregion
    
    #region AddTripUpdate
    public async Task<bool> AddTripUpdate(string TripSID, TripUpdateStatusRequestModel request)
    {
        try
        {
            var trip = await _unitOfWork.GetRepository<Trip>().SingleOrDefaultAsync(t => t.TripSid == TripSID && t.TripStatus == (int)StatusEnum.InProgress);
            if (trip == null)
            {
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
                throw new HttpStatusCodeException((int)StatusCode.BadRequest, "Invalid status");
            }

            tripUpdate.TripUpdatesStatus = request.TripUpdateStatus;
            tripUpdate.Note = request.Note;
            tripUpdate.TimeStamp = DateTime.Now;
            await _unitOfWork.GetRepository<TripUpdate>().InsertAsync(tripUpdate);
            await _unitOfWork.CommitAsync();
            return true;
        }
        catch (HttpStatusCodeException exception)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new HttpStatusCodeException((int)StatusCode.InternalServerError, exception.Message);
        }
    }
    #endregion
    
    #region TripStart
    public async Task<bool> TripsStart(string tripSID)
    {
        try
        {
            var trip = await _unitOfWork.GetRepository<Trip>().SingleOrDefaultAsync(t => t.TripSid == tripSID);
            if (trip == null)
            {
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

            return true;
        }
        catch (HttpStatusCodeException exception)
        {
            if(_unitOfWork.dbContextTransaction != null)
            {
                await _unitOfWork.dbContextTransaction.RollbackAsync();
            }
            throw;
        }
        catch (Exception exception)
        {
            if(_unitOfWork.dbContextTransaction != null)
            {
                await _unitOfWork.dbContextTransaction.RollbackAsync();
            }
            throw new HttpStatusCodeException((int)StatusCode.InternalServerError, exception.Message);
        }
    }
    #endregion
    
    #region TripEnd
    public async Task<bool> TripsEnd(string tripSID)
    {
        try
        {
            var trip = await _unitOfWork.GetRepository<Trip>().SingleOrDefaultAsync(t => t.TripSid == tripSID);
            if (trip == null)
            {
                throw new HttpStatusCodeException((int)StatusCode.NotFound, "No results found");
            }

            var driverCurrentLocation = await _unitOfWork.GetRepository<DriverCurrentLocation>()
                .SingleOrDefaultAsync(t => t.TripId == trip.TripId);
            if (driverCurrentLocation == null)
            {
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

            return true;
        }
        catch (HttpStatusCodeException exception)
        {
            if(_unitOfWork.dbContextTransaction != null)
            {
                await _unitOfWork.dbContextTransaction.RollbackAsync();
            }
            throw;
        }
        catch (Exception exception)
        {
            if(_unitOfWork.dbContextTransaction != null)
            {
                await _unitOfWork.dbContextTransaction.RollbackAsync();
            }
            throw new HttpStatusCodeException((int)StatusCode.InternalServerError, exception.Message);
        }
    }
    #endregion
    
    #region GetAllTripUpdateStatus
    public async Task<List<TripUpdateResponseModel>> GetAllTripUpdateStatus(string tripSID)
    {
        try
        {
            string query = "sp_GetTripUpdates {0}";
            object[] param = { tripSID };
            var res = await _spContext.ExecuteStoreProcedure(query, param);
            List<TripUpdateResponseModel> tripUpdateResponseModels = JsonConvert.DeserializeObject<List<TripUpdateResponseModel>>(res?.ToString() ?? "[]");
            if (res == null)
            {
                throw new HttpStatusCodeException((int)StatusCode.NotFound, "No results found");
            }
            return tripUpdateResponseModels;
        }
        catch (HttpStatusCodeException exception)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new HttpStatusCodeException((int)StatusCode.InternalServerError, exception.Message);
        }
    }
    #endregion

    #region DeteleteTrip

    public async Task<bool> DeleteTrip(string tripSID)
    {
        try
        {
            var trip = await _unitOfWork.GetRepository<Trip>()
                .SingleOrDefaultAsync(t => t.TripSid == tripSID);
            if (trip == null)
            {
                throw new HttpStatusCodeException((int)StatusCode.NotFound, "No results found");
            }

            if (trip.TripStatus != (int)StatusEnum.Pending)
            {
                throw new HttpStatusCodeException((int)StatusCode.NotFound, "Can not Delete the Trip");
            }
            
            trip.Status = (int)StatusEnum.Delete;
            _unitOfWork.GetRepository<Trip>().Update(trip);
            await _unitOfWork.CommitAsync();
            return true;
        }
        catch (HttpStatusCodeException exception)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new HttpStatusCodeException((int)StatusCode.InternalServerError, exception.Message);
        }
    }

    #endregion

    #region UpdateTrip

    public async Task<bool> UpdateTrip(string tripSID, TripRequestModel model)
    {
        try
        {
            var t = await _unitOfWork.GetRepository<Trip>().SingleOrDefaultAsync(t => t.TripSid == tripSID);
            if (t == null)
            {
                throw new HttpStatusCodeException((int)StatusCode.NotFound, "No results found");
            }
            if (t.TripStatus != (int)StatusEnum.Pending)
            {
                throw new HttpStatusCodeException((int)StatusCode.BadRequest, "You cannot Update The Trip");
            }
            var sLocation = await _unitOfWork.GetRepository<Location>()
                .SingleOrDefaultAsync(l => l.LocationSid == model.StartLocationSID);
            if (sLocation == null)
            {
                throw new HttpStatusCodeException((int)StatusCode.BadRequest,"Start location not found");
            }
            var eLocation = await _unitOfWork.GetRepository<Location>()
                .SingleOrDefaultAsync(l => l.LocationSid == model.ToLocationSID);
            if (eLocation == null)
            {
                throw new HttpStatusCodeException((int)StatusCode.BadRequest, "To location not found");
            }

            var Driver = await _unitOfWork.GetRepository<User>()
                .SingleOrDefaultAsync(u => u.UserSid == model.DriverSID);
            if (Driver == null)
            {
                throw new HttpStatusCodeException((int)StatusCode.BadRequest, "Driver not found");
            }
            var admin = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(u => u.UserSid == model.UserSID);
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
            return true;
        }
        catch (HttpStatusCodeException exception)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new HttpStatusCodeException((int)StatusCode.InternalServerError, exception.Message);
        }
    }

    #endregion
    
}