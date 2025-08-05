using System.ComponentModel.DataAnnotations;

namespace AppTiemposV3.SharedClases.DTOs;

public class UserDto
{
    [Required]
    [Length(2, 150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [Length(5, 15)]
    public string Area { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty; 
}