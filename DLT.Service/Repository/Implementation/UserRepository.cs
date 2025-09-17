using Common;
using DLT.Models.Models.DriverLocationTracking;
using DLT.Service.Repository.Interface;
using Microsoft.AspNetCore.Http;
using Models.Models.SpDbContext;
using Models.RequestModel;
using Models.ResponsetModel;
using Service.UnitOfWork;
using Serilog; // <- Make sure you have Serilog installed

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
            Log.Information("Attempting to create user with email: {Email}", request.Email);

            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(u => u.UserEmail == request.Email);
            if (user != null)
            {
                Log.Warning("User creation failed. User with email {Email} already exists", request.Email);
                throw new HttpStatusCodeException((int)StatusCode.BadRequest, "User With Email Already Exists");
            }

            User u = new User
            {
                UserSid = "USR-" + Guid.NewGuid().ToString(),
                UserName = request.UserName,
                UserEmail = request.Email,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = (int)StatusEnum.Driver,
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
                Status = (int)StatusEnum.Acitive
            };

            await _unitOfWork.GetRepository<User>().InsertAsync(u);
            await _unitOfWork.CommitAsync();

            Log.Information("User created successfully with email: {Email}", request.Email);
            return true;
        }
        catch (HttpStatusCodeException e)
        {
            Log.Error(e, "HTTP error occurred while creating user with email: {Email}", request.Email);
            throw;
        }
        catch (Exception e)
        {
            Log.Error(e, "Unexpected error occurred while creating user with email: {Email}", request.Email);
            throw new HttpStatusCodeException((int)StatusCode.InternalServerError, e.Message);
        }
    }

    public async Task<TokenClaimsResponseModel> LoginUser(LoginRequestModel request)
    {
        try
        {
            Log.Information("Attempting login for user with email: {Email}", request.Email);

            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(u => u.UserEmail == request.Email);
            if (user == null)
            {
                Log.Warning("Login failed. Email not found: {Email}", request.Email);
                throw new HttpStatusCodeException((int)StatusCode.BadRequest, "Email is Not Correct");
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                Log.Warning("Login failed. Wrong password for email: {Email}", request.Email);
                throw new HttpStatusCodeException((int)StatusCode.BadRequest, "Password is Wrong");
            }

            TokenClaimsResponseModel response = new TokenClaimsResponseModel
            {
                UserSID = user.UserSid,
                Role = user.Role.ToString()
            };

            Log.Information("User login successful for email: {Email}", request.Email);
            return response;
        }
        catch (HttpStatusCodeException e)
        {
            Log.Error(e, "HTTP error occurred during login for email: {Email}", request.Email);
            throw;
        }
        catch (Exception e)
        {
            Log.Error(e, "Unexpected error occurred during login for email: {Email}", request.Email);
            throw new HttpStatusCodeException((int)StatusCode.InternalServerError, e.Message);
        }
    }
}
