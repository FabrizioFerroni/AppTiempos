using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using Microsoft.AspNetCore.Mvc;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.Api.Controllers;

[Route("auth")]
[ApiController]
public class AuthController: ControllerBase
{
    IAuthContract _authContract;

    public AuthController(IAuthContract authContract)
    {
        _authContract = authContract;
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserDto dto)
    {
        GeneralResponse? response = await _authContract.Register(dto);
        return StatusCode(201, response);
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        LoginResponse? response = await _authContract.Login(dto);
        return Ok(response);
    }
}