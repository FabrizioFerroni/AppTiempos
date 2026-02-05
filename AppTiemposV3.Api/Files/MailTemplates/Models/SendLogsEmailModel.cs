namespace AppTiemposV3.Api.Files.MailTemplates.Models
{
    public class SendLogsEmailModel
    {
        public string AppName { get; set; } = String.Empty;
        public string UrlApp { get; set; } = String.Empty;
        public string SupportEmail { get; set; } = String.Empty;
        public string FechaEnvio { get; set; } = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        public string MachineName { get; set; } = string.Empty;
        public int ArchivosCount { get; set; } = 0;
        public int Year { get; set; } = DateTime.UtcNow.Year;
        public string? LogoUrl { get; set; } = String.Empty;
    }
}
