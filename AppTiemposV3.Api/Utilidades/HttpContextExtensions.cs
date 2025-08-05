using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AppTiemposV3.Api.Utilidades;

public static class HttpContextExtensions
{
    public async static Task InsertarParametrosPaginacionEnCabezera<T>(this HttpContext httpContext, IQueryable<T> queryable)
    {
        if (httpContext == null)
        {
            throw new ArgumentNullException(nameof(httpContext));
        }

        double cantidad = await queryable.CountAsync();
        httpContext.Response.Headers.Append("cantidad-total-registros", cantidad.ToString());
    }
}