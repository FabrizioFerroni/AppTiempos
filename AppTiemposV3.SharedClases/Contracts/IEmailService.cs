namespace AppTiemposV3.SharedClases.Contracts;

public interface IEmailService
{
    Task<bool> Send(string to, string subject, string html, string? from);
    
    Task<string> GetEmailTemplateAsync<T>(string emailTemplate, T emailTemplateModel, bool fromEmbedded = false);
}