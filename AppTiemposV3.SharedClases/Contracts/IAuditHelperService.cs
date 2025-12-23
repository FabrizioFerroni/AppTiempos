using AppTiemposV3.SharedClases.DTOs.Audits;

namespace AppTiemposV3.SharedClases.Contracts;

public interface IAuditHelperService
{
    Task CreateAuditAsync(
        string userFullName,
        string actionActivity,
        string action,
        string entity,
        string entityName,
        List<AuditChangeDto>? changes = null,
        Dictionary<string, string>? metadata = null
    );
}