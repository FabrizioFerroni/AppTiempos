using System.ComponentModel.DataAnnotations;
using AppTiemposV3.SharedClases.Enums;

namespace AppTiemposV3.SharedClases.DTOs.Users;

public class UserResponseDto
{
    [Required]
    [Length(2, 150)]
    public string FullName { get; set; } = string.Empty;

    public Areas Area { get; set; }

    [Required]
    [EmailAddress]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = string.Empty;
    
    //public string? ImageUrl { get; set; } = string.Empty;
    public string? Rol { get; set; } = string.Empty;
    
    public bool TwoFactorEnable  { get; set; } = false;
}