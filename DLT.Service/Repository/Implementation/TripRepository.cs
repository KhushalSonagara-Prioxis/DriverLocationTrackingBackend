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
    public async Task<Page> GetAllTrips(Dictionary<string, object> parameters)
    {
        try
        {
            var xmlParams = Common.CommonHelper.DictionaryToXml(parameters, "Search");
            string query = "sp_SearchTrips {0}";
            object[] param = { xmlParams };
            var res = await _spContext.ExecutreStoreProcedureResultList(query, param);
            if (res == null)
            {
                throw new HttpStatusCodeException((int)StatusCode.NotFound, "No results found");
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
            t.TripSid = "TRI-" + Guid.NewGuid().ToString().Substring(0, 48);
            t.StartLatitude = model.StartLatitude;
            t.StartLongitude = model.StartLongitude;
            t.StartLocation = sLocation.LocationId;
        }
        catch (HttpStatusCodeException exception)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new HttpStatusCodeException((int)StatusCode.InternalServerError, exception.Message);
        }
        return true;
    }

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
}