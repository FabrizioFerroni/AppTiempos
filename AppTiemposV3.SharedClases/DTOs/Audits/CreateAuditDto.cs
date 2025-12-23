using System.ComponentModel.DataAnnotations;

namespace AppTiemposV3.SharedClases.DTOs.Audits;

public class CreateAuditDto
{
    public string UserName { get; set; } = null!;
    public string Action { get; set; } = null!;
    public string Entity { get; set; } = null!;
    public string EntityId { get; set; } = null!;
    public string EntityName { get; set; } = null!;
    public required string ActionActivity { get; set; }
    public List<AuditChangeDto> Changes { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
}


public class AuditChangeDto
{
    public string Field { get; set; } = null!;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}