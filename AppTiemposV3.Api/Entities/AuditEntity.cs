using System.ComponentModel.DataAnnotations;

namespace AppTiemposV3.Api.Entities;

public class AuditEntity : BaseEntity
{
    [Required]
    public required Guid UserId { get; set; }
    [Required]
    public required string ActionActivity { get; set; }
    public UserEntity User { get; set; } = null!;
    public string UserName { get; set; } = null!;

    public string Action { get; set; } = null!;

    public string Entity { get; set; } = null!;
    public string EntityId { get; set; } = null!;
    public string EntityName { get; set; } = null!;

    public List<AuditChange> Changes { get; set; } = new();

    public Dictionary<string, string> Metadata { get; set; } = new();

    public string IpAddress { get; set; } = null!;
}

public class AuditChange
{
    public string Field { get; set; } = null!;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}