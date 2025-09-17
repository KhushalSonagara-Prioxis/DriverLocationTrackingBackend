using DemoProject.Controllers;
using DLT.Service.Repository.Implementation;
using DLT.Service.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.RequestModel;

namespace DLT.Api.Controllers;

public class UserController : BaseController
{
    private readonly IUserRepository _userRepository;
    private readonly AuthenticationRepository  _authenticationRepository;
    public UserController(IUserRepository userRepository, AuthenticationRepository authenticationRepository)
    {
        _userRepository = userRepository;
        _authenticationRepository = authenticationRepository;
    }

    [HttpPost("SignUp")]
    public async Task<ActionResult> SignUp([FromBody] SignUpRequestModel request)
    {
        var success = await _userRepository.CreateUser(request);
        if (!success)
        {
            return BadRequest();
        }
        return Ok(new { Message = "User created successfully" });
    }

    [HttpPost("Login")]
    public async Task<ActionResult> Login([FromBody] LoginRequestModel request)
    {
        var res = await _userRepository.LoginUser(request);
        if (res == null)
        {
            return BadRequest();
        }
        var token = _authenticationRepository.GenerateToken(res.UserSID, res.Role);
        return Ok(new { Message = "User logged in successfully",token = token,role = res.Role });
    }
    

}