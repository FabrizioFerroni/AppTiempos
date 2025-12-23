namespace AppTiemposV3.Api.Helpers
{
    public static class EntityNavigationHelper
    {
        public static bool HasNavigations<TEntity>()
        {
            return typeof(TEntity)
                .GetProperties()
                .Any(p =>
                    // ICollection<T>
                    (p.PropertyType.IsGenericType &&
                     p.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>))

                    ||

                    // Referencia a otra Entity
                    (p.PropertyType.IsClass &&
                     p.PropertyType != typeof(string) &&
                     p.PropertyType.Name.EndsWith("Entity"))
                );
        }
    }
}
