using System.Reflection;
using System.Text;
using AppTiemposV3.SharedClases.Contracts;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using RazorLight;
using static System.IO.File;

namespace AppTiemposV3.Api.Services;

public class EmailService: IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly RazorLightEngine _razor;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
        _razor = new RazorLightEngineBuilder()
            .UseEmbeddedResourcesProject(Assembly.GetExecutingAssembly()) // para recursos embebidos
            .UseMemoryCachingProvider()
            .Build();
    }
    
    public async Task<bool> Send(string to, string subject, string html, string? from)
    {
       try
       {
           string emailFrom = _configuration["Email:EmailFrom"] ?? Environment.GetEnvironmentVariable("Email__EmailFrom")!;
           string smtpHost = _configuration["Email:SmtpHost"] ?? Environment.GetEnvironmentVariable("Email__SmtpHost")!;
           string smtpUser = _configuration["Email:SmtpUser"] ?? Environment.GetEnvironmentVariable("Email__SmtpUser")!;
           string smtpPassword = _configuration["Email:SmtpPass"] ?? Environment.GetEnvironmentVariable("Email__SmtpPass")!;
           int smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? Environment.GetEnvironmentVariable("Email__SmtpPort") ?? "587");
           
           MimeMessage email = new MimeMessage();
           email.From.Add(MailboxAddress.Parse(emailFrom ?? from));
           email.To.Add(MailboxAddress.Parse(to));
           email.Subject = subject;
           email.Body = new TextPart(TextFormat.Html) { Text = html };

           using SmtpClient smtp = new SmtpClient();
           await smtp.ConnectAsync(
               smtpHost,
               smtpPort,
               SecureSocketOptions.StartTls
           );

           await smtp.AuthenticateAsync(
               smtpUser,
               smtpPassword
           );

           await smtp.SendAsync(email);
           await smtp.DisconnectAsync(true);

           return true;
       }
       catch (Exception ex)
       {
           Console.WriteLine($"Error enviando email: {ex.Message}");
           return false;
       }
    }
    
    public async Task<string> GetEmailTemplateAsync<T>(string emailTemplate, T emailTemplateModel, bool fromEmbedded = false)
    {
        if (fromEmbedded)
        {
            // Cargar desde recurso embebido
            string resourceKey = $"AppTiemposV3.Api.Files.MailTemplates.{emailTemplate}.cshtml";
            return await _razor.CompileRenderAsync(resourceKey, emailTemplateModel);
        }
        else
        {
            // Cargar desde disco
            string template = LoadTemplateFromDisk(emailTemplate);
            return await _razor.CompileRenderStringAsync(emailTemplate, template, emailTemplateModel);
        }
    }

    private string LoadTemplateFromDisk(string emailTemplate)
    {
        string templateDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files", "MailTemplates");
        string templatePath = Path.Combine(templateDir, $"{emailTemplate}.cshtml");
        return ReadAllText(templatePath, Encoding.UTF8);
    }
}