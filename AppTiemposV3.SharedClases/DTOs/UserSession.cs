using AppTiemposV3.SharedClases.Enums;

namespace AppTiemposV3.SharedClases.DTOs;

public record UserSession(Guid? Id, string? FullName, Areas? Area, string? Email, string? Role, DateTime? LastPasswordChange);