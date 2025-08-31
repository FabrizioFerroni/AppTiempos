using AppTiemposV3.Api.Entities;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.Api.Controllers;

[Route("auth")]
[ApiController]
public class AuthController : ControllerBase
{
    IAuthContract _authContract;

    public AuthController(IAuthContract authContract)
    {
        _authContract = authContract;
    }

    [HttpPost("invite")]
    public async Task<IActionResult> Invite([FromBody] InviteDto dto)
    {
        GeneralResponse? response = await _authContract.Invite(dto);
        return Ok(response);
    }

    [HttpPost("register/{token}")]
    public async Task<IActionResult> Register(string token,[FromBody] UserDto dto)
    {
        GeneralResponse? response = await _authContract.Register(token, dto);
        return StatusCode(201, response);
    }

    [HttpPost("accept-invite/{id}")]
    public async Task<IActionResult> AcceptInvite(Guid id, [FromBody] AcceptInviteDto dto)
    {
        GeneralResponse? response = await _authContract.AcceptInvitation(id, dto);
        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        StringValues origin = Request.Headers["origin"];
        LoginResponse? response = await _authContract.Login(dto, origin!);
        return Ok(response);
    }

    [HttpPost("login/2fa")]
    public async Task<IActionResult> Login2Fa([FromBody] Login2FA dto)
    {
        LoginResponse? response = await _authContract.Login2FA(dto);
        return Ok(response);
    }

    [HttpPost("forgotpassword")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        GeneralResponse? response = await _authContract.ForgotPassword(dto);
        return Ok(response);
    }

    [HttpPost("resetpassword/{token}")]
    public async Task<IActionResult> ResetPassword(string token, ResetPasswordDto dto)
    {
        GeneralResponse? response = await _authContract.ResetPassword(token, dto);
        return Ok(response);
    }

    [HttpPost("activatetwofactor")]
    public async Task<IActionResult> Activate2Fa([FromBody] Activate2FA dto)
    {
        GeneralResponse? response = await _authContract.activate2FA(dto);
        return Ok(response);
    }
}