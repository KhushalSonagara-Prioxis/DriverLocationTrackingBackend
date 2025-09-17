using Common;
using DLT.Models.Models.DriverLocationTracking;
using DLT.Service.Repository.Interface;
using Microsoft.AspNetCore.Http;
using Models.Models.SpDbContext;
using Models.RequestModel;
using Models.ResponsetModel;
using Service.UnitOfWork;

namespace DLT.Service.Repository.Implementation;

public class UserRepository : IUserRepository
{
    private readonly DriverLocationTrackingDbContext _context;
    private readonly DriverLocationTrackingSpContext _spContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserRepository(DriverLocationTrackingDbContext context, DriverLocationTrackingSpContext spContext,
        IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _spContext = spContext;
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> CreateUser(SignUpRequestModel request)
    {
        try
        {
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(u => u.UserEmail == request.Email);
            if (user != null)
            {
                throw new HttpStatusCodeException((int)StatusCode.BadRequest,"User With Email Already Exists");
            }

            User u = new User();
            u.UserSid = "USR-" + Guid.NewGuid().ToString();
            u.UserName = request.UserName;
            u.UserEmail = request.Email;
            u.PhoneNumber = request.PhoneNumber;
            string hashPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
            u.PasswordHash = hashPassword;
            u.Role = (int)StatusEnum.Driver;
            u.CreatedDate = DateTime.Now;
            u.LastModifiedDate = DateTime.Now;
            u.Status = (int)StatusEnum.Acitive;
            await _unitOfWork.GetRepository<User>().InsertAsync(u);
            await _unitOfWork.CommitAsync();
            return true;
        }
        catch (HttpStatusCodeException e)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new HttpStatusCodeException((int)StatusCode.InternalServerError, e.Message);
        }
    }

    public async Task<TokenClaimsResponseModel> LoginUser(LoginRequestModel request)
    {
        try
        {
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(u => u.UserEmail == request.Email);
            if (user == null)
            {
                throw new HttpStatusCodeException((int)StatusCode.BadRequest,"Email is Not Correct");
            }
            bool isPasseordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isPasseordValid)
            {
                throw new HttpStatusCodeException((int)StatusCode.BadRequest,"Password is Wrong");
            }
            TokenClaimsResponseModel response = new TokenClaimsResponseModel();
            response.UserSID = user.UserSid;
            response.Role = user.Role.ToString();
            return response;
        }
        catch (HttpStatusCodeException e)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new HttpStatusCodeException((int)StatusCode.InternalServerError, e.Message);
        }
    }
}