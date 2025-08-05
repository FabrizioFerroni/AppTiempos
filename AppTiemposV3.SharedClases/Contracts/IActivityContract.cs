using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Activities;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.SharedClases.Contracts;

public interface IActivityContract<T>
{
    Task<DataAResponse<T>> GetAllActivities(Guid userId);

    Task<Pageable<List<T>>> GetAllActivitiesPag(PaginationDto pagination, string buscarPor, Guid userId);
    
    Task<Pageable<List<T>>> GetAllActivitiesPerDayPag(PaginationDto pagination, DateOnly startDate, Guid userId);
    
    Task<Pageable<List<T>>> GetAllActivitiesPerRangePag(PaginationDto pagination, DateOnly startDate, DateOnly endDate, Guid userId);
    
    Task<DataAResponse<T>> GetLastThreeActivities(Guid userId);
    
    Task<DataResponse<T>> GetActivityById(Guid id, Guid userId);
    
    Task<DataResponse<T>> GetActivityByUrl(string url, Guid userId);

    Task<GeneralResponse> CreateActivity(CreateActivityDto dto, Guid userId);
    
    Task<GeneralResponse> UpdateActivity(Guid id, UpdateActivityDto dto, Guid userId);
    
    Task<GeneralResponse> DeleteActivity(Guid id, Guid userId);
    
    Task<GeneralResponse> RestoreActivity(Guid id, Guid userId);
}