using System.ComponentModel.DataAnnotations;

namespace AppTiemposV3.SharedClases.DTOs;

public class InviteDto
{
    [Required]
    public required string FullName { get; set; }
    
    [Required]
    [EmailAddress]
    [DataType(DataType.EmailAddress)]
    public required string Email { get; set; }
    
  
    [Required, MinLength(15)]
    [DataType(DataType.MultilineText)]
    public required string Reason { get; set; }
}
