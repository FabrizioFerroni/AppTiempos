namespace AppTiemposV3.Api.Files.MailTemplates.Models;

public class TwoFactorEmailModel
{
    public string AppName { get; set; } = "Mi App";
    public string UrlApp { get; set; } = "https://tusitio.com";
    public string Token { get; set; } = "123456";
    public int Minutes { get; set; } = 10;
    public string SupportEmail { get; set; } = "soporte@tusitio.com";
    public string UnsubscribeUrl { get; set; } = "https://tusitio.com/unsubscribe";
    public int Year { get; set; } = DateTime.UtcNow.Year;
}