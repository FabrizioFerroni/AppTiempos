using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AppTiemposV3.SharedClases.DTOs;

public class LoginDto
{
    [Required]
    [EmailAddress]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
        
    [JsonIgnore]
    public bool RememberMe { get; set; } = false;
}