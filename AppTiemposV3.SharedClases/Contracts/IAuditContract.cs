using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Audits;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.SharedClases.Contracts;

public interface IAuditContract<T>
{
    Task<Pageable<List<T>>> GetAllAudits(PaginationDtoAdvanced pagination);
    
    Task<DataAResponse<AuditUserResponseDto>> GetLastFourAuditsUser();

    Task<DataResponse<AuditKpiResponse>> GetKpis();

    Task<GeneralResponse> CreateAudit(CreateAuditDto dto);
}