using Models.RequestModel;
using Models.ResponsetModel;

namespace DLT.Service.Repository.Interface;

public interface ILocationRepository
{
    Task<List<LocationResponseModel>> GetAllLocation();
    Task<LocationResponseModel> AddLocation(LocationRequestModel model);
    Task<bool> DeleteLocation(string locationSID);
}