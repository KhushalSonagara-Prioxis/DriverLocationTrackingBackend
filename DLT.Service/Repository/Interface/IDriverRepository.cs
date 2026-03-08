using Models.Models.CommonModel;
using Models.RequestModel;
using Models.ResponsetModel;

namespace DLT.Service.Repository.Interface;

public interface IDriverRepository
{
    Task<bool> UpdateDriverCurrectLocationAsync(string TripSID, DriverCurrentLocationRequestModel driverCurrentLocation);
    Task<DriverCurrectLocationResponseModel> GetDriverCurrectLocationAsync(string TripSID);
    Task<List<DriverDropDownResponseModel>> GetAllDriversDropDown();
    Task<Page> GetAllTripsOfDrivers(Dictionary<string, object> parameters);

    Task<Page> GetDriverList(Dictionary<string, object> parameters);
    Task<DriverDetailsResponseModel> GetDriverDetails(string driverSid);

    Task<bool> ActiveInactiveDriver(string driverSid);
    Task<bool> DeleteDriver(string driverSid);
}