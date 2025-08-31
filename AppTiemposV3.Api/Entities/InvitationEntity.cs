using System.ComponentModel.DataAnnotations;

namespace AppTiemposV3.Api.Entities;

public class InvitationEntity : BaseEntity
{
    [Required]
    public string FullName { get; set; }
    
    [Required]
    [EmailAddress]
    [DataType(DataType.EmailAddress)]
    public string Email {  get; set; }
    
    [Required, MinLength(15)]
    [DataType(DataType.MultilineText)]
    public string Reason { get; set; } 
    
    public bool Accepted { get; set; } = false;
    
    public bool Finished { get; set; } = false;
    
    [Required]
    public required string Token { get; set; }
    
    public DateTime DateReceived { get; set; } = DateTime.Now;
    
    public DateTime? DateSent { get; set; } = null;

}