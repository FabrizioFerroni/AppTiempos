using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.Api.Controllers;

[Authorize(Roles = "Admin, User")]
[Route("api/dashboard")]
[ApiController]
public class DashboardController: ControllerBase 
{
    private readonly IDashboardContract<DashboardKPIDto>  _dashContract;

    public DashboardController(IDashboardContract<DashboardKPIDto> dashContract)
    {
        _dashContract = dashContract;
    }
    
    [HttpGet("kpi/{yearQ}/{weekNumberQ}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAllActivitiesPaginados(string yearQ, string weekNumberQ)
    {
        int year = int.Parse(yearQ);
        int weekNumber = int.Parse(weekNumberQ);
        DataResponse<DashboardKPIDto>? response = await _dashContract.GetKpiDashboard(year, weekNumber);
        return Ok(response);
    }
}