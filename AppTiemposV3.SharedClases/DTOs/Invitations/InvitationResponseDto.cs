namespace AppTiemposV3.SharedClases.DTOs.Invitations;

public class InvitationResponseDto
{
    public Guid Id { get; set; } = Guid.Empty;
    
    public string FullName {  get; set; }
    
    public string Email {  get; set; }
    
    public string Reason { get; set; } 
    
    public bool Accepted { get; set; } = false;
    
    public bool Finished { get; set; } = false;
    
    public string Token { get; set; }
    
    public DateTime DateReceived { get; set; }
    
    public DateTime? DateSent { get; set; }
}