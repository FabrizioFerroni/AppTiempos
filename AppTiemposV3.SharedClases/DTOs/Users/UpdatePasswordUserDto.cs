using System.ComponentModel.DataAnnotations;

namespace AppTiemposV3.SharedClases.DTOs.Users;

public class UpdatePasswordUserDto
{
    [Required]
    [DataType(DataType.Password)]
    public string ActualPassword { get; set; } = string.Empty;
    
    [Required]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;
    
    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword))]
    public string ConfirmPassword { get; set; } = string.Empty;
}