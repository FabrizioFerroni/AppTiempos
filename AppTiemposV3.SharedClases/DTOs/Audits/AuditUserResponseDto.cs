namespace AppTiemposV3.SharedClases.DTOs.Audits;

public class AuditUserResponseDto
{
    public Guid Id { get; set; }
    public string Action { get; set; } = null!;
    public string? ActionActivity { get; set; }
    public string? EntityName { get; set; }
    public DateTime CreatedAt { get; set; }
    public UserDtoAu Usuario { get; set; } = new UserDtoAu();
}

public class UserDtoAu
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Area { get; set; } = string.Empty;
}