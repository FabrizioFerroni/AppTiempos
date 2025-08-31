namespace AppTiemposV3.Api.Files.MailTemplates.Models;

public class SendInvitationEmailModel
{
    public string AppName { get; set; } = String.Empty;
    public string UrlApp { get; set; } = String.Empty;
    public string FullName { get; set; } = String.Empty;
    public string Email { get; set; } = String.Empty;
    public string UrlInvitation { get; set; } = String.Empty;
    public string SupportEmail { get; set; } = String.Empty;
    
    public string Tiempo { get; set; } = String.Empty;
    public int Year { get; set; } = DateTime.UtcNow.Year;
}