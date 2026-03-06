using Models.ResponsetModel;

namespace DLT.Service.Repository.Interface;

public interface IDashboardRepository
{
    Task<DashboadResponseModel> AdminDashBoard();
}