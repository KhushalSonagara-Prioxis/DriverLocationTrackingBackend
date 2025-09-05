using Common;
using DLT.Models.Models.DriverLocationTracking;
using DLT.Service.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using Models.Models.SpDbContext;
using Models.ResponsetModel;
using Newtonsoft.Json;
using Service.UnitOfWork;

namespace DLT.Service.Repository.Implementation;

public class LocationRepository : ILocationRepository
{
    private readonly DriverLocationTrackingDbContext _context;
    private readonly DriverLocationTrackingSpContext _spContext;
    private readonly IUnitOfWork _unitOfWork;
    public LocationRepository(DriverLocationTrackingDbContext context, DriverLocationTrackingSpContext spContext, IUnitOfWork unitOfWork)
    {
        _context = context;
        _spContext = spContext;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<List<LocationResponseModel>> GetAllLocation()
    {
        try
        {
            var location = await _context.Locations.ToListAsync();
            if (location == null)
            {
                throw new HttpStatusCodeException((int)StatusCode.NotFound, "No Locations");
            }

            List<LocationResponseModel> res =
                JsonConvert.DeserializeObject<List<LocationResponseModel>>(JsonConvert.SerializeObject(location));
            return res;
        }
        catch (HttpStatusCodeException e)
        {
            throw;
        }
        catch (Exception)
        {
            throw new HttpStatusCodeException((int)StatusCode.InternalServerError, "Internal server error");
        }
       
    }
}