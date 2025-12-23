namespace AppTiemposV3.SharedClases.DTOs.Audits;

public class AuditsResponseDto
{
    public Guid Id { get; set; }
    public string Action { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public UserDtoAu Usuario { get; set; } = new UserDtoAu();
    public string UserName { get; set; } = null!;
    public string Entity { get; set; } = null!;
    public string EntityId { get; set; } = null!;
    public string EntityName { get; set; } = null!;
    public string? ActionActivity { get; set; }
    public List<AuditChangeDto> Changes { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
}