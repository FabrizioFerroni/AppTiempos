using AppTiemposV3.SharedClases.Utilidades.Interfaces;
using static NanoidDotNet.Nanoid;
using static NanoidDotNet.Nanoid.Alphabets;

namespace AppTiemposV3.SharedClases.Utilidades;

public class EntityIdProvider : IEntityIdProvider
{
    private readonly Dictionary<string, string> _entityIds = new();
    
    public async Task<string> GetOrCreate(string entityKey)
    {
        if (_entityIds.TryGetValue(entityKey, out string? entityId))
            return entityId;

        entityId = $"{entityKey.ToLower()}-{await GenerateAsync(LowercaseLettersAndDigits, 10)}";
        _entityIds[entityKey] = entityId;

        return entityId;
    }
}