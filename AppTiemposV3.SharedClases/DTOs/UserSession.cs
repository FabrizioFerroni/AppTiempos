namespace AppTiemposV3.SharedClases.DTOs;

public record UserSession(Guid? Id, string? FullName, string? Area, string? Email, string? Role, DateTime? LastPasswordChange);