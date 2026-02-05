using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.DTOs;

namespace AppTiemposV3.SharedClases.Contracts;

public interface IGenericContract
{
    Task<Pageable<List<TDto>>> GetAllPaginatedAsync<TEntity, TDto>(PaginationDto pagination, string buscarPor, Guid? userId) where TEntity : class;
    Task<Pageable<List<TEntity>>> GetAllPaginatedReportedAsync<TEntity>(PaginationDto pagination, Guid? userId) where TEntity : class;

    Task<Pageable<List<TDto>>> GetAllPaginatedPerDayAsync<TEntity, TDto>(PaginationDto pagination, DateOnly startDate, Guid? userId) where TEntity : class;
    
    Task<Pageable<List<TDto>>> GetAllPaginatedPerRangeAsync<TEntity, TDto>(PaginationDto pagination, DateOnly startDate, DateOnly? endDate, Guid? userId) where TEntity : class;
    
    Task<Pageable<List<TDto>>> GetAllPaginatedFaAsync<TEntity, TDto>(PaginationDtoAdvanced pagination, Guid? userId) where TEntity : class;
    Task<Pageable<List<TDto>>> GetAllPaginatedAuditAsync<TEntity, TDto>(PaginationDtoAdvanced pagination) where TEntity : class;
    
}