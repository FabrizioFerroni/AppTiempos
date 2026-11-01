using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Testers;
using AppTiemposV3.SharedClases.DTOs.Trainings;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.Api.Controllers;

[Authorize]
[Route("api/testers")]
[ApiController]
public class TesterController : ControllerBase
{
    private readonly ITesterContract<TesterResponseDto> _testerService;

    public TesterController(ITesterContract<TesterResponseDto> testerService)
    {
        _testerService = testerService;
    }

    [HttpGet()]
    [ProducesResponseType(200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAllTesters()
    {
        DataAResponse<TesterResponseDto> response = await _testerService.GetAllTesters();
        return Ok(response);
    }

    [HttpGet("paginated")]
    [ProducesResponseType(200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAllTestersPaginated([FromQuery] PaginationDtoAdvanced pagination)
    {
        Pageable<List<TesterResponseDto>> response = await _testerService.GetAllTestersPaginated(pagination);
        return Ok(response);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetById(Guid id)
    {
        DataResponse<TesterResponseDto> response = await _testerService.GetTesterPorId(id);
        return Ok(response);
    }

    [HttpGet("n/{nombre}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetByNombre(string nombre)
    {
        DataResponse<TesterResponseDto> response = await _testerService.GetTesterPorNombre(nombre);
        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> CreateTester([FromBody] CreateTesterDto dto)
    {
        GeneralResponse response = await _testerService.CreateTester(dto);
        return StatusCode(201, response);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> UpdateTester(Guid id, [FromBody] UpdateTesterDto dto)
    {
        GeneralResponse response = await _testerService.UpdateTester(id, dto);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> DeleteTester(Guid id)
    {
        GeneralResponse response = await _testerService.DeleteTester(id);
        return Ok(response);
    }

    [HttpPost("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> RestoreTester(Guid id)
    {
        GeneralResponse response = await _testerService.RestoreTester(id);
        return Ok(response);
    }

}
