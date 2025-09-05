using Models.ResponsetModel;

namespace DLT.Service.Repository.Interface;

public interface ILocationRepository
{
    Task<List<LocationResponseModel>> GetAllLocation();
}