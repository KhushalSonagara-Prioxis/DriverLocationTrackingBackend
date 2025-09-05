using Models.Models.CommonModel;
using Models.RequestModel;
using Models.ResponsetModel;

namespace DLT.Service.Repository.Interface;

public interface ITripRepository
{
    Task<Page> GetAllTrips(Dictionary<string, object> parameters);
    Task<bool> CreateTrip(TripRequestModel model);
    Task<bool> AddTripUpdate(string TripSID, TripUpdateStatusRequestModel tripUpdateStatusRequestModel);
    Task<List<TripUpdateResponseModel>>  GetAllTripUpdateStatus(string tripSID);
    Task<bool> TripsStart(string tripSID);
    Task<bool> TripsEnd(string tripSID);
    Task<bool> DeleteTrip(string tripSID);
    Task<bool> UpdateTrip(string tripSID, TripRequestModel tripRequestModel);
}