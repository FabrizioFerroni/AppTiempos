using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.Api.Controllers;

[Authorize]
[Route("api/users")]
[ApiController]
public class UserController: ControllerBase
{
    private readonly IUserCContract<UserResponseDto> _userCContract;

    public UserController(IUserCContract<UserResponseDto> userCContract)
    {
        _userCContract = userCContract;
    }
    
    [HttpGet("profile")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetUserLogged()
    {
        DataResponse<UserResponseDto> response = await _userCContract.GetUserLogged();
        return Ok(response);
    }


    [HttpPut("password")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> UpdatePasswordUser([FromBody] UpdatePasswordUserDto dto)
    {
        GeneralResponse response = await _userCContract.UpdateUserPassword(dto);
        return Ok(response);
    }
    
    
    [HttpPut("profile")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateUserDto dto)
    {
        GeneralResponse response = await _userCContract.UpdateUserProfile(dto);
        return Ok(response);
    }
    
    [HttpPut("2fa")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> UpdateTwoFactorUser([FromBody] EnableTwoFactorUser dto)
    {
        GeneralResponse response = await _userCContract.UpdateTwoFactor(dto);
        return Ok(response);
    }
}