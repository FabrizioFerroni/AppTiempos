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
    private readonly IUserContract _userContext;
    private Guid _userId => _userContext.GetUserId();

    public ActivityController(IActivityContract<ActivityResponseDto> activityContract, IUserContract userContext)
    {
        _activityContract = activityContract;
        _userContext = userContext;
    }
    
    [HttpGet]
    [ProducesResponseType(200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAllActivitiesPaginados([FromQuery] PaginationDto pagination, [FromQuery]  string buscarPor = "Description")
    {
        Pageable<List<ActivityResponseDto>> response = await _activityContract.GetAllActivitiesPag(pagination, buscarPor, _userId);
        return Ok(response);
    }
    
    [HttpGet("todos")]
    [ProducesResponseType(200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAllActivities()
    {
        DataAResponse<ActivityResponseDto> response = await _activityContract.GetAllActivities(_userId);
        return Ok(response);
    }
    
    [HttpGet("u/ultimos-3")]
    [ProducesResponseType(200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAllActivitiesLastThree()
    {
        DataAResponse<ActivityResponseDto> response = await _activityContract.GetLastThreeActivities(_userId);
        return Ok(response);
    }
    
    [HttpGet("date/{startDate}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAllActivitiesForDate([FromQuery] PaginationDto pagination, string startDate)
    {
        if (!DateOnly.TryParse(startDate, out var fecha))
            throw new BadRequestException("La fecha proporcionada no tiene un formato válido (ej: yyyy-MM-dd)");
        
        pagination.Ordenar = "StartDateTimeCombo";
        pagination.Ascending = false;
        
        Pageable<List<ActivityResponseDto>> response = await _activityContract.GetAllActivitiesPerDayPag(pagination, fecha, _userId);
        
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
        
        Pageable<List<ActivityResponseDto>> response = await _activityContract.GetAllActivitiesPerRangePag(pagination, fechaS, fechaE, _userId);
        
        return Ok(response);
    }

    [HttpGet("a/{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetActivityById(Guid id)
    {
        DataResponse<ActivityResponseDto> response = await _activityContract.GetActivityById(id, _userId);
        return Ok(response);
    }
    
    [HttpGet("{url}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetByActivityByUrlIndentifier(string url)
    {
        DataResponse<ActivityResponseDto> response = await _activityContract.GetActivityByUrl(url, _userId);
        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> CreateActivity([FromBody] CreateActivityDto dto)
    {
        GeneralResponse  response = await _activityContract.CreateActivity(dto, _userId);
        return StatusCode(201, response);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> UpdateActivity(Guid id, [FromBody] UpdateActivityDto dto)
    {
        GeneralResponse response = await _activityContract.UpdateActivity(id, dto, _userId);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> DeleteActivity(Guid id)
    {
        GeneralResponse response = await _activityContract.DeleteActivity(id, _userId);
        return Ok(response);
    }
    
    [HttpPost("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> RestoreActivity(Guid id)
    {
        GeneralResponse response = await _activityContract.RestoreActivity(id, _userId);
        return Ok(response);
    }
}