using System.Reflection;
using System.Text;
using AppTiemposV3.SharedClases.Contracts;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using RazorLight;
using static System.IO.File;
using static MailKit.Security.SecureSocketOptions;
using static System.IO.Path;
using static System.AppDomain;
using static MimeKit.MailboxAddress;
using static MimeKit.Text.TextFormat;
using static System.Environment;
using static System.Reflection.Assembly;

namespace AppTiemposV3.Api.Services;

public class EmailService: IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly RazorLightEngine _razor;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
        _razor = new RazorLightEngineBuilder()
            .UseEmbeddedResourcesProject(GetExecutingAssembly())
            .UseMemoryCachingProvider()
            .Build();
    }
    
    public async Task<bool> Send(string to, string subject, string html, string? from)
    {
       try
       {
           string emailFrom = _configuration["Email:EmailFrom"] ?? GetEnvironmentVariable("Email__EmailFrom")!;
           string smtpHost = _configuration["Email:SmtpHost"] ?? GetEnvironmentVariable("Email__SmtpHost")!;
           string smtpUser = _configuration["Email:SmtpUser"] ?? GetEnvironmentVariable("Email__SmtpUser")!;
           string smtpPassword = _configuration["Email:SmtpPass"] ?? GetEnvironmentVariable("Email__SmtpPass")!;
           int smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? GetEnvironmentVariable("Email__SmtpPort") ?? "587");
           
           MimeMessage email = new MimeMessage();
           email.From.Add(Parse(emailFrom ?? from));
           email.To.Add(Parse(to));
           email.Subject = subject;
           email.Body = new TextPart(Html) { Text = html };

           using SmtpClient smtp = new SmtpClient();
           await smtp.ConnectAsync(
               smtpHost,
               smtpPort,
               StartTls
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

    public async Task<bool> SendWithAttachments(List<string> destinations, string subject, string html, byte[] pdfBytes, string type, string fileName = "Reporte.pdf")
    {
        try
        {
            // 1. Obtener credenciales (Igual que ya tienes)
            string emailFrom = _configuration["Email:EmailFrom"] ?? GetEnvironmentVariable("Email__EmailFrom")!;
            string smtpHost = _configuration["Email:SmtpHost"] ?? GetEnvironmentVariable("Email__SmtpHost")!;
            string smtpUser = _configuration["Email:SmtpUser"] ?? GetEnvironmentVariable("Email__SmtpUser")!;
            string smtpPassword = _configuration["Email:SmtpPass"] ?? GetEnvironmentVariable("Email__SmtpPass")!;
            int smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? GetEnvironmentVariable("Email__SmtpPort") ?? "587");

            MimeMessage email = new MimeMessage();
            email.From.Add(Parse(emailFrom));

            foreach (string? to in destinations)
            {
                email.To.Add(Parse(to));
            }

            email.Subject = subject;

            ContentType contentType = ContentType.Parse(type);

            BodyBuilder? builder = new BodyBuilder();
            builder.HtmlBody = html;
            builder.Attachments.Add(fileName, pdfBytes, contentType);

            email.Body = builder.ToMessageBody();

            using SmtpClient smtp = new SmtpClient();
            await smtp.ConnectAsync(smtpHost, smtpPort, StartTls);
            await smtp.AuthenticateAsync(smtpUser, smtpPassword);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error enviando email con adjunto: {ex.Message}");
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
        string templateDir = Combine(CurrentDomain.BaseDirectory, "Files", "MailTemplates");
        string templatePath = Combine(templateDir, $"{emailTemplate}.cshtml");
        return ReadAllText(templatePath, Encoding.UTF8);
    }
}