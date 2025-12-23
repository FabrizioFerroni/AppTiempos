namespace AppTiemposV3.SharedClases.Utilidades.Interfaces;

public interface IEntityIdProvider
{
    Task<string> GetOrCreate(string entityKey);
}