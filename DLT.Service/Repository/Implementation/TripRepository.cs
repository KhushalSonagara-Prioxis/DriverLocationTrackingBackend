using Common;
using DLT.Models.Models.DriverLocationTracking;
using DLT.Service.Repository.Interface;
using Models.Models.CommonModel;
using Models.Models.SpDbContext;
using Service.RepositoryFactory;
using Service.UnitOfWork;

namespace DLT.Service.Repository.Implementation;

public class TripRepository : ITripRepository
{
    private readonly DriverLocationTrackingDbContext _context;
    private readonly DriverLocationTrackingSpContext _spContext;
    private readonly IUnitOfWork _unitOfWork;
    public TripRepository(DriverLocationTrackingDbContext context, DriverLocationTrackingSpContext spContext, IUnitOfWork unitOfWork)
    {
        _context = context;
        _spContext = spContext;
        _unitOfWork = unitOfWork;
    }
    public async Task<Page>  GetAllTrips(Dictionary<string, object> parameters)
    {
        try
        {
            var xmlParams = Common.CommonHelper.DictionaryToXml(parameters, "Search");
            string query = "sp_SearchTrips {0}";
            object[] param = { xmlParams };
            var res = await _spContext.ExecutreStoreProcedureResultList(query, param);
            if (res == null)
            {
                throw new HttpStatusCodeException((int)StatusCode.NotFound, "No results found");
            }

            return res;
        }
        catch (HttpStatusCodeException exception)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new HttpStatusCodeException((int)StatusCode.InternalServerError, exception.Message);
        }
    }
}