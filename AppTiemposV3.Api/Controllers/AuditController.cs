using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Audits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.Api.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/audits")]
[ApiController]
public class AuditController : ControllerBase
{
    private readonly IAuditContract<AuditsResponseDto>  _auditService;

    public AuditController(IAuditContract<AuditsResponseDto> auditService)
    {
        _auditService = auditService;
    }
    
    [HttpGet("kpi")]
    [ProducesResponseType(200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetKpiAudits()
    {
        DataResponse<AuditKpiResponse> response = await _auditService.GetKpis();
        return Ok(response);
    }
    
    
    [HttpGet("ultimate-audits")]
    [Authorize(Roles = "Admin, User")]
    [ProducesResponseType(200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetLastFourAudits()
    {
        DataAResponse<AuditUserResponseDto> response = await _auditService.GetLastFourAuditsUser();
        return Ok(response);
    }
    
    [HttpGet]
    [ProducesResponseType(200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAllAuditsPaginados([FromQuery] PaginationDtoAdvanced pagination)
    {
        Pageable<List<AuditsResponseDto>> response = await _auditService.GetAllAudits(pagination);
        return Ok(response);
    }
}