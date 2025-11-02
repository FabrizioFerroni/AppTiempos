using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Activities;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.SharedClases.Contracts;

public interface IActivityContract<T>
{
    Task<DataAResponse<T>> GetAllActivities();

    Task<Pageable<List<T>>> GetAllActivitiesPag(PaginationDto pagination, string buscarPor);
    
    Task<Pageable<List<T>>> GetAllActivitiesPerDayPag(PaginationDto pagination, DateOnly startDate);
    Task<DataAResponse<T>> GetAllActivitiesPerDay(DateOnly startDate);
    
    Task<Pageable<List<T>>> GetAllActivitiesPerRangePag(PaginationDto pagination, DateOnly startDate, DateOnly endDate);
    
    Task<DataAResponse<T>> GetLastThreeActivities();
    
    Task<DataResponse<T>> GetActivityById(Guid id);
    
    Task<DataResponse<T>> GetActivityByUrl(string url);
    
    Task<DataResponse<Guid>> GetRequerimentActivity(string reqId);

    Task<GeneralResponse> CreateActivity(CreateActivityDto dto);
    
    Task<GeneralResponse> UpdateActivity(Guid id, UpdateActivityDto dto);
    
    Task<GeneralResponse> DeleteActivity(Guid id);
    
    Task<GeneralResponse> RestoreActivity(Guid id);
}