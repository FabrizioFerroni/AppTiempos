
namespace AppTiemposV3.SharedClases.Contracts;

public interface IEmailService
{
    Task<bool> Send(string to, string subject, string html, string? from);
    Task<bool> SendWithAttachments(List<string> destinations, string subject, string html, byte[] pdfBytes, string type, string fileName = "Reporte.pdf");
    Task<string> GetEmailTemplateAsync<T>(string emailTemplate, T emailTemplateModel, bool fromEmbedded = false);
}