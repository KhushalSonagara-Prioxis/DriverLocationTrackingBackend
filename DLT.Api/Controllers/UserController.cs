using DemoProject.Controllers;
using DLT.Service.Repository.Implementation;
using DLT.Service.Repository.Interface;
using Microsoft.AspNetCore.Mvc;
using Models.RequestModel;
using Serilog;

namespace DLT.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : BaseController
{
    private readonly IUserRepository _userRepository;
    private readonly AuthenticationRepository _authenticationRepository;

    public UserController(IUserRepository userRepository, AuthenticationRepository authenticationRepository)
    {
        _userRepository = userRepository;
        _authenticationRepository = authenticationRepository;
    }

    [HttpPost("SignUp")]
    public async Task<ActionResult> SignUp([FromBody] SignUpRequestModel request)
    {
        Log.Information("SignUp request received for Email: {Email}", request.Email);

        try
        {
            var success = await _userRepository.CreateUser(request);
            if (!success)
            {
                Log.Warning("Failed to create user with Email: {Email}", request.Email);
                return BadRequest(new { Message = "User creation failed" });
            }

            Log.Information("User created successfully with Email: {Email}", request.Email);
            return Ok(new { Message = "User created successfully" });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Exception occurred while creating user with Email: {Email}", request.Email);
            throw;
        }
    }

    [HttpPost("Login")]
    public async Task<ActionResult> Login([FromBody] LoginRequestModel request)
    {
        Log.Information("Login attempt for Email: {Email}", request.Email);

        try
        {
            var res = await _userRepository.LoginUser(request);
            if (res == null)
            {
                Log.Warning("Invalid login attempt for Email: {Email}", request.Email);
                return BadRequest(new { Message = "Invalid credentials" });
            }

            var token = _authenticationRepository.GenerateToken(res.UserSID, res.Role);

            Log.Information("Login successful for UserSID: {UserSID}, Role: {Role}", res.UserSID, res.Role);
            return Ok(new { Message = "User logged in successfully", token = token, role = res.Role });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Exception occurred during login for Email: {Email}", request.Email);
            throw;
        }
    }
}
