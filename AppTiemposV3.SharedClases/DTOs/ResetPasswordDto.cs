using System.ComponentModel.DataAnnotations;

namespace AppTiemposV3.SharedClases.DTOs;

public class ResetPasswordDto
{
    [Required]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;
    
    [Required]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}