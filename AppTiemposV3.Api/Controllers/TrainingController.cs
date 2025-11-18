using System.Text.Json;
using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Trainings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.Api.Controllers;

[Authorize]
[Route("api/trainings")]
[ApiController]
public class TrainingController : ControllerBase
{
    private readonly ITrainingContract<TrainingResponseDto> _trainingContract;

    public TrainingController(ITrainingContract<TrainingResponseDto> trainingContract)
    {
        _trainingContract = trainingContract;
    }
    
    [HttpGet("kpi")]
    [ProducesResponseType(200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetKpiTraining()
    {
        DataResponse<TrainingKpiResponse> response = await _trainingContract.GetTrainingKpi();
        return Ok(response);
    }
    
    [HttpGet]
    [ProducesResponseType(200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAllTrainingPaginados([FromQuery] PaginationDtoAdvanced pagination)
    {
        Pageable<List<TrainingResponseDto>> response = await _trainingContract.GetAllTrainings(pagination);
        return Ok(response);
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetById(Guid id)
    {
        DataResponse<TrainingResponseDto> response = await _trainingContract.GetTrainingPorId(id);
        return Ok(response);
    }
    
    [HttpPost]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> CreateTraining([FromBody] CreateTrainingDto dto)
    {
        GeneralResponse  response = await _trainingContract.CreateTraining(dto);
        return StatusCode(201, response);
    }
    
    [HttpPut("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> UpdateTraining(Guid id, [FromBody] UpdateTrainingDto dto)
    {
        GeneralResponse response = await _trainingContract.UpdateTraining(id, dto);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> DeleteTraining(Guid id)
    {
        GeneralResponse response = await _trainingContract.DeleteTraining(id);
        return Ok(response);
    }
    
    [HttpPost("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> RestoreTraining(Guid id)
    {
        GeneralResponse response = await _trainingContract.RestoreTraining(id);
        return Ok(response);
    }
}