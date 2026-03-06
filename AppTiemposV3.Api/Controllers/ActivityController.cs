using AppTiemposV3.Api.Entities;
using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Activities;
using AppTiemposV3.SharedClases.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.Api.Controllers;

[Authorize(Roles = "Admin, User")]
[Route("api/activities")]
[ApiController]
public class ActivityController : ControllerBase
{
    private readonly IActivityContract<ActivityResponseDto>  _activityContract;
    private readonly IActivityWeeklyContract<ActivitiesByDay>  _activityWeeklyContract;

    public ActivityController(IActivityContract<ActivityResponseDto> activityContract, IActivityWeeklyContract<ActivitiesByDay>  activityWeeklyContract)
    {
        _activityContract = activityContract;
        _activityWeeklyContract = activityWeeklyContract;
    }
    
    [HttpGet]
    [ProducesResponseType(200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAllActivitiesPaginados([FromQuery] PaginationDto pagination, [FromQuery]  string buscarPor = "Description")
    {
        Pageable<List<ActivityResponseDto>> response = await _activityContract.GetAllActivitiesPag(pagination, buscarPor);
        return Ok(response);
    }
    
    [HttpGet("todos")]
    [ProducesResponseType(200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAllActivities()
    {
        DataAResponse<ActivityResponseDto> response = await _activityContract.GetAllActivities();
        return Ok(response);
    }
    
    [HttpGet("u/ultimos-3/{yearQ}/{weekNumberQ}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAllActivitiesLastThree(string yearQ, string weekNumberQ)
    {
        int year = int.Parse(yearQ);
        int weekNumber = int.Parse(weekNumberQ);
        DataAResponse<ActivityResponseDto> response = await _activityContract.GetLastThreeActivities(year, weekNumber);
        return Ok(response);
    }
    
    [HttpGet("date/{startDate}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAllActivitiesForDate([FromQuery] PaginationDto pagination, string startDate)
    {
        if (!DateOnly.TryParse(startDate, out var fecha))
            throw new BadRequestException("La fecha proporcionada no tiene un formato válido (ej: yyyy-MM-dd)");
        
        if(pagination.Ordenar == "")
        {
            pagination.Ordenar = "StartDateTimeCombo";
        }
        
        Pageable<List<ActivityResponseDto>> response = await _activityContract.GetAllActivitiesPerDayPag(pagination, fecha);
        
        return Ok(response);
    }
    
    [HttpGet("t/date/{startDate}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAllActivitiesForDateSPag(string startDate)
    {
        if (!DateOnly.TryParse(startDate, out var fecha))
            throw new BadRequestException("La fecha proporcionada no tiene un formato válido (ej: yyyy-MM-dd)");
        
        DataAResponse<ActivityResponseDto> response = await _activityContract.GetAllActivitiesPerDay(fecha);
        
        return Ok(response);
    }
    
    [HttpGet("range/{startDate}/{endDate}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAllActivitiesForRange([FromQuery] PaginationDto pagination, string startDate, string endDate)
    {
        if (!DateOnly.TryParse(startDate, out var fechaS))
            throw new BadRequestException("La fecha proporcionada no tiene un formato válido (ej: yyyy-MM-dd)");

        if (!DateOnly.TryParse(endDate, out var fechaE))
            throw new BadRequestException("La fecha proporcionada no tiene un formato válido (ej: yyyy-MM-dd)");
        
        pagination.Ordenar = "StartDateTimeCombo";
        pagination.Ascending = false;
        
        Pageable<List<ActivityResponseDto>> response = await _activityContract.GetAllActivitiesPerRangePag(pagination, fechaS, fechaE);
        
        return Ok(response);
    }
    
    [HttpGet("weekly/{yearQ}/{weekNumberQ}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAllActivitiesForRangeGetAllActivitiesPerRangeWeekC(string yearQ, string weekNumberQ)
    {
        int year = int.Parse(yearQ);
        int weekNumber = int.Parse(weekNumberQ);
        
        DataAResponse<ActivitiesByDay> response = await _activityWeeklyContract.GetAllActivitiesPerRangeWeek(year, weekNumber);
        
        return Ok(response);
    }

    [HttpGet("a/{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetActivityById(Guid id)
    {
        DataResponse<ActivityResponseDto> response = await _activityContract.GetActivityById(id);
        return Ok(response);
    }
    
    [HttpGet("{url}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetByActivityByUrlIndentifier(string url)
    {
        DataResponse<ActivityResponseDto> response = await _activityContract.GetActivityByUrl(url);
        return Ok(response);
    }
    
    [HttpGet("r/{reqId}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetByRequerimentByReqId(string reqId)
    {
        DataResponse<Guid> response = await _activityContract.GetRequerimentActivity(reqId);
        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> CreateActivity([FromBody] CreateActivityDto dto)
    {
        GeneralResponse  response = await _activityContract.CreateActivity(dto);
        return StatusCode(201, response);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> UpdateActivity(Guid id, [FromBody] UpdateActivityDto dto)
    {
        GeneralResponse response = await _activityContract.UpdateActivity(id, dto);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> DeleteActivity(Guid id)
    {
        GeneralResponse response = await _activityContract.DeleteActivity(id);
        return Ok(response);
    }
    
    [HttpPost("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> RestoreActivity(Guid id)
    {
        GeneralResponse response = await _activityContract.RestoreActivity(id);
        return Ok(response);
    }
}