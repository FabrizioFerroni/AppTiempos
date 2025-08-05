using System.Linq.Expressions;
using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Requeriments;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.SharedClases.Contracts
{
    public interface IRequerimentContract<T>
    {
        Task<DataAResponse<T>> GetAllRequeriments(Guid userId);

        Task<Pageable<List<T>>> GetAllRequerimentsPag(PaginationDto pagination, string buscarPor, Guid userId);

        Task<DataResponse<T>> GetRequerimentporId(Guid id, Guid userId);
        
        Task<DataResponse<T>> GetRequerimentporReqId(string reqId, Guid userId);

        Task<GeneralResponse> CreateRequeriment(CreateRequerimentDto dto, Guid userId);

        Task<GeneralResponse> UpdateRequeriment(Guid id, UpdateRequerimentDto dto, Guid userId);

        Task<GeneralResponse> DeleteRequeriment(Guid id, Guid userId);
        
        Task<GeneralResponse> RestoreRequeriment(Guid id, Guid userId);
    }
}
