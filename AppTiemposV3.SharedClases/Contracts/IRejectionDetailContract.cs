using AppTiemposV3.SharedClases.DTOs.RejectionDetails;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.SharedClases.Contracts;

public interface IRejectionDetailContract<T>
{
    Task<DataResponse<T>> GetRejectionDetailPorId(Guid id);
    
    Task<GeneralResponse> CreateRejectionDetail(CreateRejectionDetailDto dto);

    Task<GeneralResponse> UpdateRejectionDetail(Guid id, UpdateRejectionDetailDto dto);

    Task<GeneralResponse> DeleteRejectionDetail(Guid id);
        
    Task<GeneralResponse> RestoreRejectionDetail(Guid id);

}