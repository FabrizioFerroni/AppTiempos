using System.Linq.Expressions;
using AppTiemposV3.Api.Entities;
using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Requeriments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.Api.Controllers;

[Authorize]
[Route("api/requeriments")]
[ApiController]
public class RequerimentController : ControllerBase
{
    private readonly IRequerimentContract<RequerimentResponseDto> _requerimentContract;
    private readonly IUserContract _userContext;
    private Guid _userId => _userContext.GetUserId();

    public RequerimentController(IRequerimentContract<RequerimentResponseDto> requerimentContract, IUserContract userContext)
    {
        _requerimentContract = requerimentContract;
        _userContext = userContext;
    }
    
    [HttpGet]
    [ProducesResponseType(200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAllRequerimentPaginados([FromQuery] PaginationDto pagination, [FromQuery]  string buscarPor = "ReqID")
    {
        Pageable<List<RequerimentResponseDto>> response = await _requerimentContract.GetAllRequerimentsPag(pagination, buscarPor, _userId);
        return Ok(response);
    }
    
    [HttpGet("todos")]
    [ProducesResponseType(200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAllRequeriment()
    {
        DataAResponse<RequerimentResponseDto> response = await _requerimentContract.GetAllRequeriments(_userId);
        return Ok(response);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetById(Guid id)
    {
        DataResponse<RequerimentResponseDto> response = await _requerimentContract.GetRequerimentporId(id, _userId);
        return Ok(response);
    }
    
    [HttpGet("req/{reqId}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetByReqId(string reqId)
    {
        DataResponse<RequerimentResponseDto> response = await _requerimentContract.GetRequerimentporReqId(reqId, _userId);
        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> CreateRequeriment([FromBody] CreateRequerimentDto dto)
    {
        GeneralResponse  response = await _requerimentContract.CreateRequeriment(dto, _userId);
        return StatusCode(201, response);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> UpdateRequeriment(Guid id, [FromBody] UpdateRequerimentDto dto)
    {
        GeneralResponse response = await _requerimentContract.UpdateRequeriment(id, dto, _userId);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> DeleteRequeriment(Guid id)
    {
        GeneralResponse response = await _requerimentContract.DeleteRequeriment(id, _userId);
        return Ok(response);
    }
    
    [HttpPost("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> RestoreRequeriment(Guid id)
    {
        GeneralResponse response = await _requerimentContract.RestoreRequeriment(id, _userId);
        return Ok(response);
    }
}