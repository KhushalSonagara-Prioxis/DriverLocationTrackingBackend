using Models.RequestModel;
using Models.ResponsetModel;

namespace DLT.Service.Repository.Interface;

public interface IDriverRepository
{
    Task<bool> UpdateDriverCurrectLocationAsync(string TripSID, DriverCurrentLocationRequestModel driverCurrentLocation);
    Task<DriverCurrectLocationResponseModel> GetDriverCurrectLocationAsync(string TripSID);
}