using AppTiemposV3.Api.Entities;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs.Audits;
using AppTiemposV3.SharedClases.Exceptions;
using AppTiemposV3.SharedClases.Utilidades.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace AppTiemposV3.Api.Services;

public class AuditHelperService : IAuditHelperService
{
    private readonly IAuditContract<AuditsResponseDto> _auditContract;
    private readonly IEntityIdProvider _entityIdProvider;

    public AuditHelperService(IAuditContract<AuditsResponseDto> auditContract, IEntityIdProvider entityIdProvider)
    {
        _auditContract = auditContract;
        _entityIdProvider = entityIdProvider;
    }

    public async Task CreateAuditAsync(string userFullName, string actionActivity, string action, string entity, string entityName, List<AuditChangeDto>? changes = null,
        Dictionary<string, string>? metadata = null)
    {
        
        CreateAuditDto dto = new CreateAuditDto
        {
            UserName = userFullName ?? "Usuario desconocido",
            Action = action,
            ActionActivity = actionActivity,
            Changes = changes ?? new List<AuditChangeDto>(),
            Metadata = metadata ?? new Dictionary<string, string>(),
            Entity = entity,
            EntityName = entityName,
            EntityId = await _entityIdProvider.GetOrCreate(entityName)
        };

        await _auditContract.CreateAudit(dto);
    }
}