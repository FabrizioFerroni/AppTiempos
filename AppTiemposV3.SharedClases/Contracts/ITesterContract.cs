using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Testers;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.SharedClases.Contracts;

public interface ITesterContract<TDto>
{
    Task<DataAResponse<TDto>> GetAllTesters();
    Task<Pageable<List<TDto>>> GetAllTestersPaginated(PaginationDtoAdvanced pagination);
    Task<DataResponse<TDto>> GetTesterPorId(Guid id);
    Task<DataResponse<TDto>> GetTesterPorNombre(string nombre);
    Task<GeneralResponse> CreateTester(CreateTesterDto dto);
    Task<GeneralResponse> UpdateTester(Guid id, UpdateTesterDto dto);
    Task<GeneralResponse> DeleteTester(Guid id);
    Task<GeneralResponse> RestoreTester(Guid id);

}
