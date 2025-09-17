using Models.Models.CommonModel;
using Models.RequestModel;
using Models.ResponsetModel;

namespace DLT.Service.Repository.Interface;

public interface IDriverRepository
{
    Task<bool> UpdateDriverCurrectLocationAsync(string TripSID, DriverCurrentLocationRequestModel driverCurrentLocation);
    Task<DriverCurrectLocationResponseModel> GetDriverCurrectLocationAsync(string TripSID);
    Task<List<DriverResponseModel>> GetAllDrivers();
    Task<Page> GetAllTripsOfDrivers(Dictionary<string, object> parameters);
}