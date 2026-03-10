using AppTiemposV3.SharedClases.DTOs.RequerimentsAttachments;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.SharedClases.Contracts
{
    public interface IRequerimentAttachmentContract<T>
    {
        Task<DataAResponse<T>> GetAllRAttachments();
        Task<DataAResponse<T>> GetAllRAttachmentsByRequerimentId(Guid requerimentId);
        Task<DataResponse<T>> GetRAttachmentById(Guid id);
        Task<GeneralResponse> CreateRAttachment(CreateOrUpdateRequerimentAttachmentDto dto, byte[] fileBytes, string fileName);
        Task<GeneralResponse> UpdateRAttachment(Guid id, CreateOrUpdateRequerimentAttachmentDto dto, byte[] fileBytes, string fileName);
        Task<GeneralResponse> DeleteRAttachment(Guid id);
    }
}
