using AppTiemposV3.Api.Entities;
using AppTiemposV3.Api.Entities.ConfigurationTable;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs.Configurations;
using DocumentFormat.OpenXml.Bibliography;
using MySql.Data.MySqlClient;
using Quartz;
using static System.IO.Directory;
using static System.IO.Path;

namespace AppTiemposV3.Api.Scheduled
{
    public class BackupsBDJob : IJob
    {
        private readonly IConfiguration _configuration;
        private readonly IBackupContract _backupContract;
        private readonly ILogger<BackupsBDJob> _logger;
        private readonly IWebHostEnvironment _env;

        public BackupsBDJob(IConfiguration configuration, IBackupContract backupContract, ILogger<BackupsBDJob> logger, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _backupContract = backupContract;
            _logger = logger;
            _env = env;
        }


        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Revisando backups programados: {Time}", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));

            try
            {
                List<BackupScheduledJobDto>? response = await _backupContract.GetAllConfigs();

                if (response == null || !response.Any()) return;

                foreach (BackupScheduledJobDto? backupSchedule in response)
                {
                    BackupScheduled? backup = backupSchedule?.BackupScheduled;
                    if (backup == null || !backup.AutoBackup) continue;

                    // 1. Validar Hora (HH:mm)
                    DateTime ahora = DateTime.Now;
                    TimeSpan horaActual = ahora.TimeOfDay;
                    TimeSpan horaProg = TimeSpan.Parse(backup.Time!);

                    bool esHora = horaActual >= horaProg && horaActual < horaProg.Add(TimeSpan.FromMinutes(5));
                    if (!esHora) continue;

                    //if (ahora.ToString("HH:mm") != backup.Time) continue;

                    // 2. Validar Frecuencia
                    bool debeEjecutarse = backup.Frecuencia!.ToLower() switch
                    {
                        "diario" => true,
                        "semanal" => ahora.DayOfWeek == DayOfWeek.Sunday,
                        "mensual" => ahora.Day == 1,
                        _ => false
                    };

                    if (!debeEjecutarse) continue;

                    // 3. Proceso de Backup Físico
                    try
                    {
                        _logger.LogInformation("Iniciando backup físico: {Frecuencia}", backup.Frecuencia);

                        string connectionString = _configuration.GetConnectionString("MySQL")
                                                 ?? throw new InvalidOperationException("Falta la cadena de conexión MySQL");

                        // Ruta en el servidor (ContentRootPath)
                        string folderPath = Combine(_env.ContentRootPath, "Backups");

                        if (!Directory.Exists(folderPath)) CreateDirectory(folderPath);

                        string fileName = $"Backup_{backup.Frecuencia}_{ahora:yyyyMMdd_HHmmss}.sql";
                        string filePath = Combine(folderPath, fileName);

                        using (MySqlConnection conn = new MySqlConnection(connectionString))
                        {
                            using (MySqlCommand cmd = new MySqlCommand { Connection = conn })
                            {
                                using (MySqlBackup mb = new MySqlBackup(cmd))
                                {
                                    await conn.OpenAsync();
                                    mb.ExportToFile(filePath);
                                }
                            }
                        }

                        FileInfo fileInfo = new FileInfo(filePath);
                        long fileSizeInBytes = fileInfo.Length;
                        double fileSizeInMb = fileSizeInBytes / 1024.0 / 1024.0; 

                        await _backupContract.GuardarRegistroBackupenBD(fileSizeInBytes, filePath, backupSchedule!.UserId, backupSchedule.ConfigId);

                        ApplyRetentionPolicy(folderPath, backup.MaxBackup);
                        _logger.LogInformation("Backup completado. Tamaño: {Size:N2} MB en Ruta: {Path}", fileSizeInMb, filePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al generar el archivo SQL para {Frecuencia}", backup.Frecuencia);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error general en el Job de Backups.");
            }
        }




        private void ApplyRetentionPolicy(string folder, int? maxFiles)
        {
            DirectoryInfo? directory = new DirectoryInfo(folder);

            List<FileInfo>? files = directory.GetFiles("*.sql")
                                 .OrderByDescending(f => f.CreationTime)
                                 .ToList();

            if (files.Count > maxFiles)
            {
                IEnumerable<FileInfo>? filesToDelete = files.Skip(maxFiles ?? 3);
                foreach (FileInfo? file in filesToDelete)
                {
                    try
                    {
                        file.Delete();
                        _logger.LogInformation("Archivo excedente eliminado: {Name}", file.Name);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("No se pudo borrar el archivo {Name}: {Msg}", file.Name, ex.Message);
                    }
                }
            }
        }

        
    }
}
