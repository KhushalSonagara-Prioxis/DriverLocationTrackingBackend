using Models.RequestModel;
using Models.ResponsetModel;

namespace DLT.Service.Repository.Interface;

public interface IDashboardRepository
{
    Task<DashboardTileResponseModel> AdminDashBoard();

    Task<AdminDashboardBarChartResponseModel> AdminDashBoardBarChart(AdminDashBoardChartRequestModel requestModel);
}