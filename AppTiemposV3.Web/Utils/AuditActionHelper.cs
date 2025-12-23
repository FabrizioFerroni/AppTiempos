using AppTiemposV3.SharedClases.Enums;

namespace AppTiemposV3.Web.Utils;

public static class AuditActionHelper
{
    public static string Normalize(string action)
    {
        return action switch
        {
            "Creado" => AuditAction.Created.ToString(),
            "Actualizado" => AuditAction.Updated.ToString(),
            "Eliminado" => AuditAction.Deleted.ToString(),
            "Estado Cambiado" => AuditAction.StatusChanged.ToString(),
            "Etapa Cambiada" => AuditAction.StageChanged.ToString(),
            "Completado" => AuditAction.Completed.ToString(),
            "Rechazado" => AuditAction.Rejected.ToString(),
            "Aprobado" => AuditAction.Approved.ToString(),
            "Asignado" => AuditAction.Assigned.ToString(),
            _ => action
        };
    }
    
    private static readonly Dictionary<AuditAction, string> Texts = new()
    {
        { AuditAction.Created, "Creado" },
        { AuditAction.Updated, "Actualizado" },
        { AuditAction.Deleted, "Eliminado" },
        { AuditAction.StatusChanged, "Estado Cambiado" },
        { AuditAction.StageChanged, "Etapa Cambiada" },
        { AuditAction.Completed, "Completado" },
        { AuditAction.Rejected, "Rechazado" },
        { AuditAction.Approved, "Aprobado" },
        { AuditAction.Assigned, "Asignado" },
    };

    public static string Get(string action)
    {
        if (Enum.TryParse(action, out AuditAction parsed))
            return Texts[parsed];

        return action;
    }
}