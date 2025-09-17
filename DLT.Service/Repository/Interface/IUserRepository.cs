using Models.RequestModel;
using Models.ResponsetModel;

namespace DLT.Service.Repository.Interface;

public interface IUserRepository
{
    Task<bool> CreateUser(SignUpRequestModel request);
    Task<TokenClaimsResponseModel> LoginUser(LoginRequestModel request);
}