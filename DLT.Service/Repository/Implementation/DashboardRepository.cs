using Common;
using DLT.Models.Models.DriverLocationTracking;
using DLT.Service.Repository.Interface;
using Microsoft.AspNetCore.Http;
using Models.Models.SpDbContext;
using Models.ResponsetModel;
using Models.RequestModel;
using Newtonsoft.Json;
using Serilog;
using Service.RepositoryFactory;
using Service.UnitOfWork;

namespace DLT.Service.Repository.Implementation;

public class DashboardRepository : IDashboardRepository
{
    private readonly DriverLocationTrackingDbContext _context;
    private readonly DriverLocationTrackingSpContext _spContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DashboardRepository(DriverLocationTrackingDbContext context, DriverLocationTrackingSpContext spContext, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _spContext = spContext;
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }
    public async Task<DashboardTileResponseModel> AdminDashBoard()
    {
        var trips = await _unitOfWork
            .GetRepository<Trip>()
            .GetAllAsync(x => x.Status != (int)StatusEnum.Delete);
        
        var drivers = await _unitOfWork
            .GetRepository<User>()
            .GetAllAsync(x => x.Status != (int)StatusEnum.Delete && x.Role == (int)StatusEnum.Driver);

        DashboardTileResponseModel dashboardResponse = new DashboardTileResponseModel
        {
            TotalNumberOfTrips = trips?.Count() ?? 0,
            CompletedTrips = trips?.Count(t => t.TripStatus == (int)StatusEnum.Completed) ?? 0,
            InProgressTrips = trips?.Count(t => t.TripStatus == (int)StatusEnum.InProgress) ?? 0,
            PendingTrips = trips?.Count(t => t.TripStatus == (int)StatusEnum.Pending) ?? 0,
            NumberOfDriver = drivers?.Count() ?? 0
        };

        return dashboardResponse;
    }

    public async Task<AdminDashboardBarChartResponseModel> AdminDashBoardBarChart(AdminDashBoardChartRequestModel requestModel)
    {
       
        try
        {
            string query = "sp_GetDashboardBarCharts_JSON @StartDate = {0}, @EndDate = {1}";
            object[] param = { requestModel.StartDate, requestModel.EndDate };
            var res = await _spContext.ExecuteStoreProcedure(query, param);

            AdminDashboardBarChartResponseModel response =
                JsonConvert.DeserializeObject<AdminDashboardBarChartResponseModel>(res?.ToString() ?? "{}");

            if (response == null)
            {
                Log.Warning("No trip details found");
                throw new HttpStatusCodeException((int)StatusCode.NotFound, "Trip not found");
            }

            return response;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}