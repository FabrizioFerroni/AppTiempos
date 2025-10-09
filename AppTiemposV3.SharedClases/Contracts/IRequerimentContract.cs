using System.Linq.Expressions;
using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Requeriments;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.SharedClases.Contracts
{
    public interface IRequerimentContract<T>
    {
        Task<DataAResponse<T>> GetAllRequeriments();

        Task<Pageable<List<T>>> GetAllRequerimentsPag(PaginationDto pagination, string buscarPor);

        Task<DataResponse<T>> GetRequerimentporId(Guid id);
        
        Task<DataResponse<T>> GetRequerimentporReqId(string reqId);

        Task<GeneralResponse> CreateRequeriment(CreateRequerimentDto dto);

        Task<GeneralResponse> UpdateRequeriment(Guid id, UpdateRequerimentDto dto);

        Task<GeneralResponse> DeleteRequeriment(Guid id);
        
        Task<GeneralResponse> RestoreRequeriment(Guid id);
    }
}
