namespace AppTiemposV3.Api.Files.MailTemplates.Models
{
    public class ReportScheduledEmailModel
    {
        public string AppName { get; set; } = String.Empty;
        public string UrlApp { get; set; } = String.Empty;
        public string FullName { get; set; } = String.Empty;
        public string NameReport { get; set; } = String.Empty;
        public string Email { get; set; } = String.Empty;
        public string SupportEmail { get; set; } = String.Empty;
        public int Year { get; set; } = DateTime.UtcNow.Year;
        public string? UrlReport { get; set; } = String.Empty;
        public string? LogoUrl { get; set; } = String.Empty;
    }
}
