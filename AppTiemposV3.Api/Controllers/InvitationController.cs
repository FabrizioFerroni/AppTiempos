using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Invitations;
using AppTiemposV3.SharedClases.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.Api.Controllers;


[Authorize(Roles = "Admin")]
[Route("api/invitations")]
[ApiController]
public class InvitationController : ControllerBase
{
    private readonly IInvitationContract<InvitationResponseDto>  _invContract;

    public InvitationController(IInvitationContract<InvitationResponseDto> invContract)
    {
        _invContract = invContract;
    }
    
    [HttpGet]
    [ProducesResponseType(200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAllInvitations([FromQuery] PaginationDtoAdvanced pagination)
    {
        Pageable<List<InvitationResponseDto>> response = await _invContract.GetAllInvitations(pagination);
        return Ok(response);
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetById(Guid id)
    {
        DataResponse<InvitationResponseDto> response = await _invContract.GetInvitationPorId(id);
        return StatusCode((int)response.Status, response);
    }
    
    [HttpGet("token/{token}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetByToken(string token)
    {
        DataResponse<InvitationResponseDto> response = await _invContract.GetInvitationPorToken(token);
        return StatusCode((int)response.Status, response);
    }
    
    [HttpGet("verify/{token}")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> VerifyByToken(string token)
    {
        DataResponse<EstadosInvitaciones> response = await _invContract.VerifyInvitation(token);
        return StatusCode((int)response.Status, response);
    }

    
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> CreateInvitation([FromBody] CreateInvitationDto dto)
    {
        GeneralResponse response = await _invContract.CreateInvitation(dto);
        return StatusCode(201, response);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> UpdateInvitation(Guid id, [FromBody] AcceptOrDeclineInvitationDto dto)
    {
        GeneralResponse response = await _invContract.AcceptOrDeclineInvitation(id, dto);
        return Ok(response);
    }
    
    [HttpDelete("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> DeleteInvitation(Guid id)
    {
        GeneralResponse response = await _invContract.DeleteInvitation(id);
        return Ok(response);
    }
}