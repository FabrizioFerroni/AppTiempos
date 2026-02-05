using AppTiemposV3.Api.Files.MailTemplates.Models;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs.Reports;
using static System.TimeZoneInfo;
using Quartz;
using static AppTiemposV3.SharedClases.Utilidades.GenerateSlug;
using static System.Environment;

namespace AppTiemposV3.Api.Scheduled
{
    public class ReportScheduled : IJob
    {
        private readonly IReportScheduledContract _reportServiceScheduled;
        private readonly IReportContract _reportService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ReportScheduled> _logger;

        public ReportScheduled(IReportScheduledContract reportServiceScheduled, IReportContract reportService, IEmailService emailService, IConfiguration configuration, ILogger<ReportScheduled> logger)
        {
            _reportServiceScheduled = reportServiceScheduled;
            _reportService = reportService;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Iniciando proceso de reportes programados a las {Time}", DateTime.Now);

            try
            {
                List<ReportScheduledDto>? reportes = await _reportServiceScheduled.GetAllScheduledReports();

                if (reportes == null || !reportes.Any())
                {
                    _logger.LogWarning("No se encontraron reportes para procesar.");
                    return;
                }

                TimeZoneInfo argZone = FindSystemTimeZoneById("America/Argentina/Cordoba");
                DateTime hoy = ConvertTimeFromUtc(DateTime.UtcNow, argZone);

                foreach (ReportScheduledDto? reporte in reportes)
                {
                    if (reporte.IsScheduled && DebeEnviarseHoy(reporte.Frecuency!, hoy))
                    {
                        _logger.LogInformation("Procesando reporte: {ReportName} (Url ID: {urlId}) para usuario: {userId}", reporte.Name, reporte.UrlId, reporte.UserId);

                        try
                        {
                            byte[] pdfBytes = await _reportServiceScheduled.GeneratePDFScheduled(reporte.UrlId, reporte.UserId);
                            _logger.LogDebug("PDF generado exitosamente para el reporte {urlId}", reporte.UrlId);

                            // Combinar destinatarios y nombres
                            IEnumerable<(string e, string n)> recipients = reporte.Destinations!.Zip(reporte.FullName!, (e, n) => (e, n));

                            foreach ((string email, string name) in recipients)
                            {
                                _logger.LogInformation("Enviando email a {Email} ...", email);

                                ReportScheduledEmailModel? reportDto = new ReportScheduledEmailModel
                                {
                                    AppName = _configuration["appName"] ?? GetEnvironmentVariable("appName")!,
                                    UrlApp = _configuration["urlFront"] ?? GetEnvironmentVariable("urlFront")!,
                                    FullName = name,
                                    NameReport = reporte.Name,
                                    Email = email,
                                    Year = hoy.Year,
                                    SupportEmail = _configuration["emailSoporte"] ?? GetEnvironmentVariable("emailSoporte")!,
                                    LogoUrl = _configuration["logoUrl"] ?? GetEnvironmentVariable("logoUrl")!
                                };

                                string body = await _emailService.GetEmailTemplateAsync("sendReport", reportDto);

                                bool enviado = await _emailService.SendWithAttachments(
                                    new List<string> { $"{name} <{email}>" },
                                    "Envío de reporte solicitado",
                                    body,
                                    pdfBytes,
                                    "application/pdf",
                                    $"{URLFriendly(reporte.Name)}-{hoy:yyyyMMdd}.pdf"
                                );

                                if (enviado)
                                    _logger.LogInformation("Email enviado correctamente a {Email}", email);
                                else
                                    _logger.LogError("Fallo el envío de email a {Email}", email);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error critico procesando el reporte {UrlId}", reporte.UrlId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Error general en el Job de reportes programados");
            }

            _logger.LogInformation("Finalizo el proceso de reportes programados a las {Time}", DateTime.Now);
        }

        private bool DebeEnviarseHoy(string frecuencia, DateTime fecha)
        {
            return frecuencia switch
            {
                "Diario" => true,
                "Semanal (Lunes)" => fecha.DayOfWeek == DayOfWeek.Monday,
                "Mensual (Dia 1)" => fecha.Day == 1,
                _ => false
            };

        }
    }
}
