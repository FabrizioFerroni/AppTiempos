namespace AppTiemposV3.SharedClases.DTOs.Invitations;

public class InvitationFilterDto
{
 public string? TypeNameOrEmail { get; set; }
 public string? NameOrEmail { get; set; }
 public DateTime? DateReceivedFrom { get; set; }
 public DateTime? DateReceivedTo { get; set; }
}