using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Rejections;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.SharedClases.Contracts;

public interface IRejectionContract<T>
{
    Task<DataResponse<RejectionKpiResponse>> GetRejectionKpi();
    
    Task<Pageable<List<T>>> GetAllRejections(PaginationDtoAdvanced pagination);
    
    Task<DataResponse<T>> GetRejectionPorId(Guid id);
    Task<DataResponse<T>> GetRejectionPorUrl(string url);
    
    Task<DataResponse<CreateRejectionResponseDto>> CreateRejection(CreateRejectionDto dto);

    Task<GeneralResponse> UpdateRejection(Guid id, UpdateRejectionDto dto);
    Task<GeneralResponse> UpdateRejectionCount(Guid id, UpdateRejectionCountDto dto);

    Task<GeneralResponse> DeleteRejection(Guid id);
        
    Task<GeneralResponse> RestoreRejection(Guid id);

}