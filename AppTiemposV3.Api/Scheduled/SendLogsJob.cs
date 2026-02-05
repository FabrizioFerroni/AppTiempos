using AppTiemposV3.Api.Files.MailTemplates.Models;
using AppTiemposV3.SharedClases.Contracts;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using Quartz;
using System.IO.Compression;
using static System.TimeZoneInfo;
using static System.Environment;

namespace AppTiemposV3.Api.Scheduled
{
    public class SendLogsJob : IJob
    {
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SendLogsJob> _logger;

        public SendLogsJob(IEmailService emailService, IConfiguration configuration, ILogger<SendLogsJob> logger)
        {
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Iniciando compactacion de logs...");

            string rootPath = Directory.GetCurrentDirectory();
            string logsPath = Path.Combine(rootPath, "Logs");
            string zipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LogsBackup.zip");

            _logger.LogInformation("Buscando logs en: {Path}", logsPath);

            try
            {
                if (!Directory.Exists(logsPath))
                {
                    Directory.CreateDirectory(logsPath);
                    _logger.LogWarning("La carpeta de logs no existía y fue creada. No hay archivos para enviar.");
                    return;
                }

                // Eliminar zip anterior si existe
                if (File.Exists(zipPath)) File.Delete(zipPath);

                string hoyString = DateTime.Now.ToString("yyyyMMdd");

                string[]? archivos = Directory.GetFiles(logsPath, "*.txt");
                _logger.LogInformation("Se encontraron {Count} archivos .txt en la carpeta.", archivos.Length);

                using (ZipArchive zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                {
                    foreach (string? archivo in archivos)
                    {
                        string nombreArchivo = Path.GetFileName(archivo);

                        bool esArchivoDeHoy = nombreArchivo.Contains(DateTime.Now.ToString("yyyyMMdd"));

                        if (!esArchivoDeHoy)
                        {
                            zip.CreateEntryFromFile(archivo, nombreArchivo);
                            _logger.LogInformation("Agregado al ZIP: {Name}", nombreArchivo);
                        }
                        else
                        {
                            _logger.LogInformation("Saltando archivo de hoy (en uso): {Name}", nombreArchivo);
                        }
                    }
                }

                FileInfo zipInfo = new FileInfo(zipPath);
                if (zipInfo.Length < 100) 
                {
                    _logger.LogWarning("El ZIP esta vacio o es muy pequenio ({Size} bytes). No se enviara el correo.", zipInfo.Length);
                    return;
                }

                // Enviar por mail
                byte[] zipBytes = await File.ReadAllBytesAsync(zipPath);
                string adminEmail = _configuration["Email:AdminEmail"] ?? "admin@tuempresa.com";

                string cuerpoHtml = $@"
                    <h2>Resumen Semanal de Logs</h2>
                    <p>Se han comprimido los archivos de log de la semana pasada.</p>
                    <ul>
                        <li><b>Fecha de proceso:</b> {DateTime.Now:dd/MM/yyyy HH:mm}</li>
                        <li><b>Archivos incluidos:</b> {archivos.Length - 1}</li>
                    </ul>
                    <p>Saludos, el sistema de tareas programadas.</p>";
                TimeZoneInfo argZone = FindSystemTimeZoneById("America/Argentina/Cordoba");
                DateTime hoy = ConvertTimeFromUtc(DateTime.UtcNow, argZone);

                SendLogsEmailModel? logDto = new SendLogsEmailModel
                {
                    AppName = _configuration["appName"] ?? GetEnvironmentVariable("appName")!,
                    UrlApp = _configuration["urlFront"] ?? GetEnvironmentVariable("urlFront")!,
                    FechaEnvio = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                    MachineName = Environment.MachineName,
                    ArchivosCount = (archivos.Length - 1),
                    Year = hoy.Year,
                    SupportEmail = _configuration["emailSoporte"] ?? GetEnvironmentVariable("emailSoporte")!,
                    LogoUrl = _configuration["logoUrl"] ?? GetEnvironmentVariable("logoUrl")!
                };
                

                string body = await _emailService.GetEmailTemplateAsync("sendLogs", logDto);

                await _emailService.SendWithAttachments(
                    new List<string> { adminEmail },
                    $"Backup Logs Sistema - {DateTime.Now:dd/MM/yyyy}",
                    body,
                    zipBytes,
                    "application/zip",
                    $"Logs_{DateTime.Now:yyyyMMdd}.zip"
                );

                _logger.LogInformation("Backup de logs enviado correctamente.");

                // Limpieza: borrar el zip local después de enviar
                File.Delete(zipPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar el backup de logs");
            }
        }
    }
}
