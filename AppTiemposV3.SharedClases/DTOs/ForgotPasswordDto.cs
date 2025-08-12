using System.ComponentModel.DataAnnotations;

namespace AppTiemposV3.SharedClases.DTOs;

public class ForgotPasswordDto
{
    [Required]
    [EmailAddress]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = string.Empty;
}