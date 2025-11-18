using AppTiemposV3.SharedClases.DTOs;

namespace AppTiemposV3.Api.Utilidades;

public static class IQueryableExtensions
{
    public static IQueryable<T> Paginar<T>(this IQueryable<T> queryable, PaginationDto paginacioon)
    {
        return queryable
            .Skip((paginacioon.Pagina - 1) * paginacioon.RegistrosPorPagina)
            .Take(paginacioon.RegistrosPorPagina);
    }
    
    public static IQueryable<T> PaginarAdvanced<T>(this IQueryable<T> queryable, PaginationDtoAdvanced paginacioon)
    {
        return queryable
            .Skip((paginacioon.Pagina - 1) * paginacioon.RegistrosPorPagina)
            .Take(paginacioon.RegistrosPorPagina);
    }
}