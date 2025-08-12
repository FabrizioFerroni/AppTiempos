using System.ComponentModel.DataAnnotations;

namespace AppTiemposV3.SharedClases.DTOs;

public class TwoFactorDto {
    [Required]
    [EmailAddress]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [DataType(DataType.Text)]
    [Length(6, 6)]
    public string Code { get; set; } = string.Empty;
}