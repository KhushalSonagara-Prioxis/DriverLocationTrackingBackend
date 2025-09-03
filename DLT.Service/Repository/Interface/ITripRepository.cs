using Models.Models.CommonModel;

namespace DLT.Service.Repository.Interface;

public interface ITripRepository
{
    Task<Page> GetAllTrips(Dictionary<string, object> parameters);
}