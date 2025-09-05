using Common;
using DLT.Models.Models.DriverLocationTracking;
using DLT.Service.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using Models.Models.SpDbContext;
using Models.RequestModel;
using Models.ResponsetModel;
using Newtonsoft.Json;
using Service.RepositoryFactory;
using Service.UnitOfWork;

namespace DLT.Service.Repository.Implementation;

public class DriverRepository : IDriverRepository
{
    private readonly DriverLocationTrackingDbContext _context;
    private readonly DriverLocationTrackingSpContext _spContext;
    private readonly IUnitOfWork _unitOfWork;

    public DriverRepository(DriverLocationTrackingDbContext context, DriverLocationTrackingSpContext spContext, IUnitOfWork unitOfWork)
    {
        _context = context;
        _spContext = spContext;
        _unitOfWork = unitOfWork;
    }
    #region UpdateDriverCurrectLocationAsync
    public async Task<bool> UpdateDriverCurrectLocationAsync(string tripSID, DriverCurrentLocationRequestModel driverCurrentLocation)
    {
        try
        {
            var trip = await _unitOfWork.GetRepository<Trip>().SingleOrDefaultAsync(t => t.TripSid == tripSID); 
            if (trip == null)
            {
                throw new HttpStatusCodeException((int)StatusCode.NotFound, "Trip not found");
            }

            DriverCurrentLocation driverCurrectLocation = await _unitOfWork.GetRepository<DriverCurrentLocation>()
                .SingleOrDefaultAsync(t => t.TripId == trip.TripId);
           
            if (driverCurrectLocation == null)
            {
                throw new HttpStatusCodeException((int)StatusCode.NotFound, "Driver Currect Location not found");
            }

            driverCurrectLocation.Latitude = driverCurrentLocation.Latitude;
            driverCurrectLocation.Longitude = driverCurrentLocation.Longitude;
            driverCurrectLocation.LastUpdate = DateTime.Now;
            _unitOfWork.GetRepository<DriverCurrentLocation>().Update(driverCurrectLocation);
            await _unitOfWork.CommitAsync();
            Console.WriteLine(driverCurrectLocation.Longitude);
            Console.WriteLine(driverCurrectLocation.Latitude);
            return true;
        }
        catch (HttpStatusCodeException exception)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new HttpStatusCodeException((int)StatusCode.InternalServerError, exception);
        }
    }
    #endregion
    
    #region GetDriverCurrectLocationAsync
    public async Task<DriverCurrectLocationResponseModel> GetDriverCurrectLocationAsync(string tripSID)
    {
        try
        {
            Console.WriteLine(tripSID);
            string query = "sp_GetTripDetails {0}";
            object[] param = { tripSID };
            var res = await _spContext.ExecuteStoreProcedure(query, param);
            DriverCurrectLocationResponseModel tripDetail =
                JsonConvert.DeserializeObject<DriverCurrectLocationResponseModel>(res?.ToString() ?? "{}");
            if (tripDetail == null)
            {
                throw new HttpStatusCodeException((int)StatusCode.NotFound, "Trip not found");
            }
            return tripDetail;
        }
        catch (HttpStatusCodeException exception)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new HttpStatusCodeException((int)StatusCode.InternalServerError, exception);
        }
    }
    #endregion
    
    #region GetAllDrivers

    public async Task<List<DriverResponseModel>> GetAllDrivers()
    {
        try
        {
            var drivers = await _context.Users.Where(d => d.Role == (int)StatusEnum.Driver).ToListAsync();
            if (drivers == null || !drivers.Any())
            {
                throw new HttpStatusCodeException((int)StatusCode.NotFound, "Driver not found");
            }
            List<DriverResponseModel> res =
                JsonConvert.DeserializeObject<List<DriverResponseModel>>(JsonConvert.SerializeObject(drivers));
            if (res == null && !drivers.Any())
            {
                throw new HttpStatusCodeException((int)StatusCode.NotFound, "Driver not found");
            }
            return res;
        }
        catch (HttpStatusCodeException exception)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new HttpStatusCodeException((int)StatusCode.InternalServerError, exception);
        }
    }
    #endregion
}