using Models.Models.CommonModel;
using Models.RequestModel;

namespace DLT.Service.Repository.Interface;

public interface ITripRepository
{
    Task<Page> GetAllTrips(Dictionary<string, object> parameters);
    Task<bool> AddTripUpdate(string TripSID, TripUpdateStatusRequestModel tripUpdateStatusRequestModel);
}