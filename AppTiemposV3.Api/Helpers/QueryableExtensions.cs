using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace AppTiemposV3.Api.Helpers
{
    public static class QueryableExtensions
    {
        public static IQueryable<TEntity> IncludeAllNavigations<TEntity>(
        this IQueryable<TEntity> query)
        where TEntity : class
        {
            Type? entityType = typeof(TEntity);

            IEnumerable<PropertyInfo>? navigations = entityType
                .GetProperties()
                .Where(p =>
                    // Referencias (UserEntity, RequerimentEntity, etc.)
                    (p.PropertyType.IsClass &&
                     p.PropertyType != typeof(string) &&
                     p.PropertyType.Name.EndsWith("Entity"))

                    ||

                    // Colecciones (ICollection<T>)
                    (p.PropertyType.IsGenericType &&
                     p.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>))
                );

            foreach (PropertyInfo? navigation in navigations)
            {
                query = query.Include(navigation.Name);
            }

            return query;
        }
    }
}
