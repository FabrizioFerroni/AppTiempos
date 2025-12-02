using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Rejections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.Api.Helpers.Helpers;

namespace AppTiemposV3.Api.Controllers;

[Authorize]
[Route("api/rejections")]
[ApiController]
public class RejectionController : ControllerBase
{
    private IRejectionContract<RejectionResponseDto> _rejectionContract;

    public RejectionController(IRejectionContract<RejectionResponseDto> rejectionContract)
    {
        _rejectionContract = rejectionContract;
    }
    
    [HttpGet("kpi")]
    [ProducesResponseType(200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetKpiTraining()
    {
        DataResponse<RejectionKpiResponse> response = await _rejectionContract.GetRejectionKpi();
        return StatusCode((int)response.Status, response);
    }

    [HttpGet]
    [ProducesResponseType(200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAllRejections([FromQuery] PaginationDtoAdvanced pagination)
    {
        PrintAsJson(pagination);
        Pageable<List<RejectionResponseDto>> response = await _rejectionContract.GetAllRejections(pagination);
        return Ok(response);
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetById(Guid id)
    {
        DataResponse<RejectionResponseDto> response = await _rejectionContract.GetRejectionPorId(id);
        return StatusCode((int)response.Status, response);
    }
    
    [HttpGet("url/{url}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetByUrl(string url)
    {
        DataResponse<RejectionResponseDto> response = await _rejectionContract.GetRejectionPorUrl(url);
        return StatusCode((int)response.Status, response);
    }
    
    [HttpPost]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> CreateRejection([FromBody] CreateRejectionDto dto)
    {
        DataResponse<CreateRejectionResponseDto> response = await _rejectionContract.CreateRejection(dto);
        return StatusCode((int)response.Status, response);
    }
    
    [HttpPut("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> UpdateRejection(Guid id, [FromBody] UpdateRejectionDto dto)
    {
        GeneralResponse response = await _rejectionContract.UpdateRejection(id, dto);
        return Ok(response);
    }
    
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> DeleteRejection(Guid id)
    {
        GeneralResponse response = await _rejectionContract.DeleteRejection(id);
        return Ok(response);
    }
    
    [HttpPost("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> RestoreRejection(Guid id)
    {
        GeneralResponse response = await _rejectionContract.RestoreRejection(id);
        return Ok(response);
    }
}