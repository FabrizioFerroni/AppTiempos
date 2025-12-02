using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs.RejectionDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.Api.Controllers;

[Authorize]
[Route("api/rejections-details")]
[ApiController]
public class RejectionDetailsController : ControllerBase
{
    private readonly IRejectionDetailContract<RejectionDetailResponseDto> _rejectionDetailContract;

    public RejectionDetailsController(IRejectionDetailContract<RejectionDetailResponseDto> rejectionDetailContract)
    {
        _rejectionDetailContract = rejectionDetailContract;
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetById(Guid id)
    {
        DataResponse<RejectionDetailResponseDto> response = await _rejectionDetailContract.GetRejectionDetailPorId(id);
        return StatusCode((int)response.Status, response);
    }
    
    [HttpPost]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> CreateRejectionDetail([FromBody] CreateRejectionDetailDto dto)
    {
        GeneralResponse response = await _rejectionDetailContract.CreateRejectionDetail(dto);
        return StatusCode(201, response);
    }
    
    [HttpPut("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> UpdateRejectionDetail(Guid id, [FromBody] UpdateRejectionDetailDto dto)
    {
        GeneralResponse response = await _rejectionDetailContract.UpdateRejectionDetail(id, dto);
        return Ok(response);
    }
    
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> DeleteRejectionDetail(Guid id)
    {
        GeneralResponse response = await _rejectionDetailContract.DeleteRejectionDetail(id);
        return Ok(response);
    }
    
    [HttpPost("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> RestoreRejectionDetail(Guid id)
    {
        GeneralResponse response = await _rejectionDetailContract.RestoreRejectionDetail(id);
        return Ok(response);
    }
}