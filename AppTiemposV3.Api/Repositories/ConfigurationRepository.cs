using AppTiemposV3.Api.Data;
using AppTiemposV3.Api.Entities;
using AppTiemposV3.Api.Entities.ConfigurationTable;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs.Configurations;
using AppTiemposV3.SharedClases.Enums;
using AppTiemposV3.SharedClases.Exceptions;
using static AppTiemposV3.Api.Helpers.DatabaseHelper;
using static AppTiemposV3.Api.Helpers.Helpers;
using static AppTiemposV3.Api.Utilidades.GenerateSlug;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.SharedClases.Utilidades.DateHelper;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text;
using static NanoidDotNet.Nanoid;
using static NanoidDotNet.Nanoid.Alphabets;
using static System.Globalization.CharUnicodeInfo;
using static System.Globalization.UnicodeCategory;
using static System.IO.FileAccess;
using static System.IO.FileMode;
using static System.Net.HttpStatusCode;
using static System.StringComparison;
using static System.Text.NormalizationForm;
using MySqlConnector;

namespace AppTiemposV3.Api.Repositories
{
    public class ConfigurationRepository : IConfigurationContract
    {
        private readonly AppDbContext _dbCxt;
        private readonly UserManager<UserEntity> _userManager;
        private readonly IUserContract _userContext;
        private readonly IAuditHelperService _auditHelperService;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString = String.Empty;
        private Guid _userId => _userContext.GetUserId();
        private readonly ILogger<ConfigurationRepository> _logger;
        private readonly string[] _allowedCategoryNames =
        {
            "Nuevo",
            "Nuevo (Alta prioridad)",
            "Incidente critico",
            "Incidente no critico",
            "Nueva configuración",
            "Nueva configuración con prueba"
        };
        private record ActivitySummaryDto(
            Guid RequerimentId,
            DateOnly StartDate,
            TimeOnly StartTime,
            TimeOnly? EndTime
        );
        private record RejectionExistDto(
            Guid RejectionId,
            DateOnly RejectionDate,
            string RejectionReason
        );

        public ConfigurationRepository(AppDbContext dbCtx, UserManager<UserEntity> userManager, IUserContract userContract, IAuditHelperService auditHelperService, ILogger<ConfigurationRepository> logger, IConfiguration configuration)
        {
            _dbCxt = dbCtx;
            _userManager = userManager;
            _userContext = userContract;
            _auditHelperService = auditHelperService;
            _logger = logger;
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("MySQL") ?? throw new InvalidOperationException("Falta la cadena de conexión");
        }

        public async Task<DataResponse<ListActualConfig>> GetConfiguration()
        {
            ConfigurationEntity? config = await GetConfigActual();

            ConfigurationEntity? configWithZero = await _dbCxt.Configurations.FirstOrDefaultAsync(c => c.UserId == _userId && c.ActualConfig == 0 && c.IsDeleted == false);
            bool notUpdated = false;

            if(configWithZero is null)
            {
                notUpdated = true;
            }
            else
            {
                notUpdated = false;
            }


            ListActualConfig response = new ListActualConfig
            {
                Id = config.Id,
                ActualConfig = config.ActualConfig,
                IsNotUpdated = notUpdated,
                DayConfigs = config.DayConfigs.Select(dc => new DayConfig
                {
                    Id = dc.Id,
                    Day = (DiasSemana)dc.Day,
                    DayName = dc.DayName,
                    MinHours = dc.MinHours,
                    MaxHours = dc.MaxHours,
                    Enabled = dc.Enabled
                }).OrderBy(dc => dc.Day).ToList(),
                WeeklyPar = new DayHours
                {
                    Id = config.WeeklyParId,
                    StartTime = config.WeeklyPar.StartTime,
                    EndTime = config.WeeklyPar.EndTime
                },
                WeeklyImpar = new DayHours
                {
                    Id = config.WeeklyImparId,
                    StartTime = config.WeeklyImpar.StartTime,
                    EndTime = config.WeeklyImpar.EndTime
                },
                WorkingSaturdays = config.WorkingSaturdays.Select(ws => new WorkingSaturday
                {
                    Id = ws.Id,
                    Date = ws.Date,
                    StartTime = ws.StartTime,
                    EndTime = ws.EndTime,
                }).OrderBy(ws => ws.Date).ToList(),
                NotificationConfig = new NotificationConfigDto
                {
                    Id = config.NotificationConfigId,
                    EnableNotificationDiario = config.NotificationConfig.EnableNotificationDiario,
                    EnableNotificationSemanal = config.NotificationConfig.EnableNotificationSemanal,
                    EnableNotificationMetaAlcanzada = config.NotificationConfig.EnableNotificationMetaAlcanzada,
                    NotificationsEmail = config.NotificationConfig.NotificationsEmail,
                    HoraNotificacionDiaria = config.NotificationConfig.HoraNotificacionDiaria
                },
                BackupScheduled = new BackupScheduled
                {
                    AutoBackup = config.AutoBackupEnabled,
                    Frecuencia = config.BackupFrecuencia,
                    Time = config.BackupTime,
                    Retention = config.BackupRetention,
                    MaxBackup = config.MaxBackup
                },
                User = new UserDtoConfig
                {
                    Id = config.UserId,
                    FullName = config.User.FullName,
                    Area = config.User.Area.ToString()
                },
            };

            return new DataResponse<ListActualConfig>(true, response, OK);
        }        
        
        public async Task<GeneralResponse> CreateConfig(CreateConfigurationDto dto)
        {
            ConfigurationEntity? existingConfig = await _dbCxt.Configurations.FirstOrDefaultAsync(c => c.UserId == _userId && c.ActualConfig == 1 && c.IsDeleted == false);

            if (existingConfig is not null)
            {
                _logger.LogWarning("El usuario {UserId} ya tiene una configuración actual. No se puede crear una nueva configuración.", _userId);
                return new GeneralResponse(false, "Ya existe una configuración actual para este usuario.");

            }

            ConfigurationEntity newConfig = new ConfigurationEntity
            {
                UserId = _userId,
                ActualConfig = 1,

                AutoBackupEnabled = dto.BackupScheduled.AutoBackup,
                BackupFrecuencia = dto.BackupScheduled.Frecuencia,
                BackupTime = dto.BackupScheduled.Time,
                BackupRetention = dto.BackupScheduled.Retention,
                MaxBackup = dto.BackupScheduled.MaxBackup,

                WeeklyPar = new WeeklyHourConfig
                {
                    StartTime = dto.WeeklyPar.StartTime,
                    EndTime = dto.WeeklyPar.EndTime
                },
                WeeklyImpar = new WeeklyHourConfig
                {
                    StartTime = dto.WeeklyImpar.StartTime,
                    EndTime = dto.WeeklyImpar.EndTime
                },
                NotificationConfig = new NotificationConfigEntity
                {
                    EnableNotificationDiario = dto.NotificationConfig.EnableNotificationDiario,
                    EnableNotificationSemanal = dto.NotificationConfig.EnableNotificationSemanal,
                    EnableNotificationMetaAlcanzada = dto.NotificationConfig.EnableNotificationMetaAlcanzada,
                    NotificationsEmail = dto.NotificationConfig.NotificationsEmail,
                    HoraNotificacionDiaria = dto.NotificationConfig.HoraNotificacionDiaria
                },

                DayConfigs = dto.DayConfigs.Select(d => new DayConfigEntity
                {
                    Day = (int)d.Day,
                    DayName = d.DayName,
                    MinHours = d.MinHours,
                    MaxHours = d.MaxHours,
                    Enabled = d.Enabled
                }).ToList(),

                WorkingSaturdays = dto.WorkingSaturdays.Select(s => new WorkingSaturdayEntity
                {
                    Date = s.Date,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime
                }).ToList()
            };

            await _dbCxt.Configurations.AddAsync(newConfig);

            await EnsureSavedAsync("Hubo un error al crear la configuración.", _dbCxt);

            return new GeneralResponse(true, "Configuración creada exitosamente.");
        }

        public async Task<GeneralResponse> UpdateConfig(UpdateConfigDto dto)
        {
            bool hasBaseConfig = await _dbCxt.Configurations.AnyAsync(c => c.UserId == _userId && c.ActualConfig == 0);

            if (!hasBaseConfig)
            {
                ConfigurationEntity oldConfig = await GetConfigActual();
                oldConfig.ActualConfig = 0;
                oldConfig.ModifiedAt = DateTime.Now;
                _dbCxt.Entry(oldConfig).State = EntityState.Modified;

                await EnsureSavedAsync(
                "Hubo un error al actualizar la nueva configuracion",
                _dbCxt
            );
                ConfigurationEntity? newConfig = MapDtoToEntity(dto);
                await _dbCxt.Configurations.AddAsync(newConfig);
            }
            else
            {
                ConfigurationEntity? currentConfig = await _dbCxt.Configurations
                    .Include(c => c.WeeklyPar)
                    .Include(c => c.WeeklyImpar)
                    .Include(c => c.NotificationConfig)
                    .Include(c => c.DayConfigs)
                    .FirstOrDefaultAsync(c => c.UserId == _userId && c.ActualConfig == 1);

                if (currentConfig == null) return new GeneralResponse(false, "No se encontró configuración activa.");

                await UpdateEntityFromDto(currentConfig, dto);

                _dbCxt.Entry(currentConfig).State = EntityState.Modified;

                if (currentConfig.WeeklyPar != null)
                    _dbCxt.Entry(currentConfig.WeeklyPar).State = EntityState.Modified;

                if (currentConfig.WeeklyImpar != null)
                    _dbCxt.Entry(currentConfig.WeeklyImpar).State = EntityState.Modified;

                if (currentConfig.NotificationConfig != null)
                    _dbCxt.Entry(currentConfig.NotificationConfig).State = EntityState.Modified;

                if (currentConfig.DayConfigs != null)
                {
                    foreach (DayConfigEntity? day in currentConfig.DayConfigs)
                    {
                        _dbCxt.Entry(day).State = EntityState.Modified;
                    }
                }

                currentConfig.ModifiedAt = DateTime.Now;
                await EnsureSavedAsync("Hubo un error al actualizar la configuración", _dbCxt);
            }

            return new GeneralResponse(true, "Configuración actualizada correctamente.");
        }

        public async Task<GeneralResponse> ResetConfig()
        {
            ConfigurationEntity? currentActive = await _dbCxt.Configurations
                .Include(c => c.WeeklyPar)
                .Include(c => c.WeeklyImpar)
                .Include(c => c.NotificationConfig)
                .FirstOrDefaultAsync(c => c.UserId == _userId && c.ActualConfig == 1);

            ConfigurationEntity? baseConfig = await _dbCxt.Configurations.FirstOrDefaultAsync(c => c.UserId == _userId && c.ActualConfig == 0);

            if (baseConfig == null)
            {
                return new GeneralResponse(false, "No se encontró una configuración base para restaurar.");
            }

            if (currentActive != null)
            {
                if (currentActive.WeeklyPar != null) _dbCxt.WeeklyHourConfigs.Remove(currentActive.WeeklyPar);
                if (currentActive.WeeklyImpar != null) _dbCxt.WeeklyHourConfigs.Remove(currentActive.WeeklyImpar);
                if (currentActive.NotificationConfig != null) _dbCxt.NotificationConfigEntity.Remove(currentActive.NotificationConfig);

                _dbCxt.Configurations.Remove(currentActive);
            }

            baseConfig.ActualConfig = 1;
            baseConfig.ModifiedAt = DateTime.Now;

            _dbCxt.Entry(baseConfig).State = EntityState.Modified;

            await EnsureSavedAsync(
                "Hubo un error al restaurar la configuración original",
                _dbCxt
            );

            return new GeneralResponse(true, "Configuración restaurada a los valores originales con éxito.");
        }

        public async Task<Stream?> DownloadBackup()
        {
            string connectionString = _configuration.GetConnectionString("MySQL") ??
        throw new InvalidOperationException("Falta la cadena de conexión");

            MemoryStream? ms = new MemoryStream();
            using MySqlConnection? conn = new MySqlConnection(connectionString);
            using MySqlCommand? cmd = new MySqlCommand { Connection = conn };
            using MySqlBackup? mb = new MySqlBackup(cmd);

            await conn.OpenAsync();
            mb.ExportToMemoryStream(ms);

            long tamanoEnBytes = ms.Length;

            await GuardarRegistroBackupenBD(tamanoEnBytes);

            ms.Position = 0;
            return ms;
        }

        public async Task<DataResponse<AutoBackup>> GetLastManualBackup()
        {
            AutoBackup? lastManualBackup = await _dbCxt.BackupLogs
                .Where(bl => bl.UserId == _userId && bl.Type == "Manual" && bl.IsDeleted == false)
                .OrderByDescending(bl => bl.CreatedAt)
                .Select(bl => new AutoBackup
                {
                    Id = bl.Id,
                    Timestamp = bl.CreatedAt,
                    Size = bl.Size
                })
                .FirstOrDefaultAsync();

            return new DataResponse<AutoBackup>(true, lastManualBackup!, OK);
        }

        public async Task<DataResponse<List<AutoBackup>>> GetAutoBackupsHistory()
        {
            ConfigurationEntity? configActual = await GetConfigActual();

            List<AutoBackup>? backupsLogs = await _dbCxt.BackupLogs
                .Where(bl => bl.UserId == _userId && bl.Type == "Automatico" && bl.IsDeleted == false)
                .OrderByDescending(bl => bl.CreatedAt)
                .Select(bl => new AutoBackup
                {
                    Id = bl.Id,
                    Timestamp = bl.CreatedAt,
                    Size = bl.Size,
                    PathToBackup = Path.GetFileName(bl.PathToBackup)
                })
                .Take(configActual.MaxBackup ?? 3)
                .ToListAsync();

            return new DataResponse<List<AutoBackup>>(true, backupsLogs, OK);
        }

        public async Task<DataResponse<int>> GetTotalAutomaticBackups()
        {
            int countTotal = await _dbCxt.BackupLogs
                .Where(bl => bl.UserId == _userId 
                        && bl.Type == "Automatico" 
                        && bl.IsDeleted == false
                      )
                .OrderByDescending(bl => bl.CreatedAt)
                .CountAsync();

            return new DataResponse<int>(true, countTotal, OK);
        }

        public async Task<Stream?> DownloadFileBackup(Guid id)
        {
            try
            {
                BackupLogsEntity? backupLog = await _dbCxt.BackupLogs
                    .FirstOrDefaultAsync(x => x.Id == id); 

                if (backupLog == null)
                {
                    throw new NotFoundException("El registro de backup no existe en la base de datos.");
                }

                if (!File.Exists(backupLog.PathToBackup))
                {
                    throw new BadRequestException("El archivo físico no se encuentra en el servidor.");
                }

                FileStream? fileStream = new FileStream(
                    backupLog.PathToBackup,
                    Open,
                    Read,
                    FileShare.Read,
                    bufferSize: 4096,
                    useAsync: true);

                return fileStream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en DownloadFileBackup para el ID {Id}", id);
                throw;
            }
        }

        public async Task<GeneralResponse> RestoreBackupFromFileServer(Guid id)
        {
            try
            {
                BackupLogsEntity? backupLog = await _dbCxt.BackupLogs
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (backupLog == null || !File.Exists(backupLog.PathToBackup))
                {
                    throw new NotFoundException("No se encontró el archivo físico para realizar la restauración.");
                }

                string connString = _configuration.GetConnectionString("MySQL") ?? throw new InvalidOperationException("Falta la cadena de conexión");

                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    using (MySqlCommand cmd = new MySqlCommand())
                    {
                        using (MySqlBackup mb = new MySqlBackup(cmd))
                        {
                            cmd.Connection = conn;
                            await conn.OpenAsync();

                            mb.ImportFromFile(backupLog.PathToBackup);

                            await conn.CloseAsync();
                        }
                    }
                }

                _logger.LogInformation("Restauración completada usando el archivo: {Path}", backupLog.PathToBackup);

                return new GeneralResponse(true, "Se restauro con éxito el backup");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallo crítico en RestoreBackup para el ID {Id}", id);
                return new GeneralResponse(false, "Hubo un error al restaurar el backup");
            }
        }

        public async Task GuardarRegistroBackupenBD(long tamanoEnBytes)
        {
            ConfigurationEntity configActual = await GetConfigActual();
            BackupLogsEntity nuevoRegistro = new BackupLogsEntity
            {
                UserId = _userId,
                Size = tamanoEnBytes,
                ConfigurationEntityId = configActual.Id,
                Type = "Manual"
            };
            await _dbCxt.BackupLogs.AddAsync(nuevoRegistro);
            await EnsureSavedAsync("Error al guardar el registro de backup en la base de datos.", _dbCxt);
        }

        public async Task<GeneralResponse> RestoreFromUpload(byte[] fileBytes, string fileName)
        {
            try
            {
                using MySqlConnection? conn = new MySqlConnection(_connectionString);
                using MySqlCommand? cmd = new MySqlCommand();
                using MySqlBackup? mb = new MySqlBackup(cmd);

                cmd.Connection = conn;
                await conn.OpenAsync();

                // Usamos los bytes recibidos
                using MemoryStream? ms = new MemoryStream(fileBytes);
                mb.ImportFromStream(ms);

                await conn.CloseAsync();
                return new GeneralResponse(true, "Restauración exitosa");
            }
            catch (Exception ex)
            {
                return new GeneralResponse(false, ex.Message);
            }
        }

        public async Task<DataResponse<bool>> HasConfiguration()
        {
            ConfigurationEntity? config = await _dbCxt.Configurations.FirstOrDefaultAsync(c => c.UserId == _userId && c.ActualConfig == 1 && c.IsDeleted == false);

            return new DataResponse<bool>(true, config is not null, OK);
        }

        public async Task<GeneralResponse> ImportDataFromExcel(byte[] fileBytes, string? fileName)
        {
            try
            {
                using (MemoryStream? stream = new MemoryStream(fileBytes))
                {
                    using (XLWorkbook? workbook = new XLWorkbook(stream))
                    {
                        if (workbook.TryGetWorksheet("Categorias", out IXLWorksheet? hojaCategorias))
                        {
                            await ProcesarHojaCategoriasAsync(hojaCategorias);
                        }

                        if (workbook.TryGetWorksheet("Requerimientos", out IXLWorksheet? hojaRequerimientos))
                        {
                            await ProcesarHojaRequerimientosAsync(hojaRequerimientos);
                        }

                        if (workbook.TryGetWorksheet("Actividades", out IXLWorksheet? hojaActividad))
                        {
                            await ProcesarHojaActividadesAsync(hojaActividad);
                        }

                        if (workbook.TryGetWorksheet("Capacitaciones", out IXLWorksheet? hojaCapacitaciones))
                        {
                            await ProcesarHojaCapacitacionesAsync(hojaCapacitaciones);
                        }

                        if (workbook.TryGetWorksheet("Rechazos", out IXLWorksheet? hojaRechazos))
                        {
                            await ProcesarHojaRechazosAsync(hojaRechazos);
                        }
                    }
                }                

                return new GeneralResponse(true, "Importación finalizada");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

        }

        public async Task<DataResponse<SaturdayBannerConfigDto>> ThisWeekHaveSaturdayWork()
        {
            try
            {

                DateTime today = DateTime.Today;
                int daysUntilMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
                DateOnly startOfWeek = DateOnly.FromDateTime(today.AddDays(-daysUntilMonday));
                DateOnly endOfWeek = startOfWeek.AddDays(5);


                WorkingSaturday? workingSaturday = await _dbCxt.WorkingSaturdays
                        .Where(ws => ws.Configuration.UserId == _userId
                                  && ws.Configuration.ActualConfig == 1
                                  && !ws.Configuration.IsDeleted
                                  && ws.Date >= startOfWeek
                                  && ws.Date <= endOfWeek)
                        .Select(ws => new WorkingSaturday 
                        {
                            Id = ws.Id,
                            Date = ws.Date,
                            StartTime = ws.StartTime,
                            EndTime = ws.EndTime
                        })
                        .FirstOrDefaultAsync();
                
                if (workingSaturday == null)
                {
                    return new DataResponse<SaturdayBannerConfigDto>(true, new SaturdayBannerConfigDto { SaturdayWork = false }, OK);
                }

                string dayName = workingSaturday.Date.ToString("dddd");

                string dayNameCapitalized = char.ToUpper(dayName[0]) + dayName.Substring(1);

                SaturdayBannerConfigDto dto = new SaturdayBannerConfigDto()
                {
                    SaturdayWork = true,
                    SemanaWork = $"{startOfWeek:dd/MM/yyyy} - {endOfWeek:dd/MM/yyyy}",
                    DayWork = $"{dayNameCapitalized} ({workingSaturday.Date})",
                    HorasWork = $"{workingSaturday.StartTime:hh\\:mm} - {workingSaturday.EndTime:hh\\:mm}",
                    TotalHsWork = workingSaturday.Hours
                };

                return new DataResponse<SaturdayBannerConfigDto>(true, dto, OK);
            } catch(Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                throw;
            }
        }

        public async Task<DataResponse<ProgressHoursConfigDto>> ProgressHours()
        {
            try
            {
                ProgressHoursConfigDto resp = new ProgressHoursConfigDto();

                UserEntity user = await GetUserByIdAsync(_userId);

                StringBuilder? sb = new StringBuilder();

                sb.AppendLine("WITH MetaHoy AS (");
                sb.AppendLine("   SELECT ");
                sb.AppendLine("       CD.MinHours");;
                sb.AppendLine("   FROM configuraciones_dias AS CD");
                sb.AppendLine("   INNER JOIN configuraciones AS C ON C.Id = CD.ConfigurationEntityId");
                sb.AppendLine("   WHERE C.ActualConfig = 1");
                sb.AppendLine("   AND C.UserId = @UserId");
                sb.AppendLine("   AND CD.DayName = ELT(WEEKDAY(@DateToday) + 1, 'Lunes', 'Martes', 'Miercoles', 'Jueves', 'Viernes', 'Sabado', 'Domingo')");
                sb.AppendLine("),");
                sb.AppendLine("ProgresoReal AS (");
                sb.AppendLine("   SELECT ");
                sb.AppendLine("       COALESCE(SUM(TIMESTAMPDIFF(MINUTE, A.StartTime, A.EndTime)) / 60.0, 0) AS HorasRealizadas ");
                sb.AppendLine("   FROM activities AS A ");
                sb.AppendLine("   WHERE A.StartDate = @DateToday ");
                sb.AppendLine("   AND A.UserId = @UserId ");
                sb.AppendLine(")");
                sb.AppendLine("");
                sb.AppendLine("SELECT ");
                sb.AppendLine("   CONCAT(REPLACE(ROUND(PR.HorasRealizadas, 1), '.', ','), 'h') AS HorasRealizadasTexto,");
                sb.AppendLine("   ROUND(PR.HorasRealizadas, 1) AS HorasRealizadasDbl,");
                sb.AppendLine("   MH.MinHours AS MetaDelDia,");
                sb.AppendLine("   CASE ");
                sb.AppendLine("      WHEN MH.MinHours <= 0 THEN 0");
                sb.AppendLine("      WHEN (PR.HorasRealizadas / MH.MinHours) * 100 > 100 THEN 100");
                sb.AppendLine("      ELSE ROUND((PR.HorasRealizadas / MH.MinHours) * 100, 0)");
                sb.AppendLine("   END AS Porcentaje,");
                sb.AppendLine("   CONCAT(");
                sb.AppendLine("      FLOOR(GREATEST(0, MH.MinHours - PR.HorasRealizadas)), 'h ',");
                sb.AppendLine("      ROUND((GREATEST(0, MH.MinHours - PR.HorasRealizadas) - FLOOR(GREATEST(0, MH.MinHours - PR.HorasRealizadas))) * 60), 'min'");
                sb.AppendLine("   ) AS HorasFaltantesFormateado");
                sb.AppendLine("FROM ProgresoReal AS PR");
                sb.AppendLine("CROSS JOIN MetaHoy AS MH");

                string sql = sb.ToString();

                List<MySqlParameter> parameters = new List<MySqlParameter>();

                parameters.Add(new MySqlParameter("@UserId", user.Id));
                parameters.Add(new MySqlParameter("@DateToday", DateTime.Today.ToString("yyyy-MM-dd")));

                List<Dictionary<string, object?>> totalData = await QueryRawFullAsync(_dbCxt, sql, parameters.ToArray());

                /* foreach (Dictionary<string, object?> row in totalData)
                 {
                     resp.HorasRealizadas = row["HorasRealizadasTexto"]?.ToString() ?? "0h";
                     resp.HorasRealizadasDbl = row["HorasRealizadasDbl"] != null ? Convert.ToDouble(row["HorasRealizadasDbl"]) : 0;
                     resp.MetaDelDia = row["MetaDelDia"] != null ? Convert.ToDouble(row["MetaDelDia"]) : 0;
                     resp.Porcentaje = row["Porcentaje"] != null ? Convert.ToInt32(row["Porcentaje"]) : 0;
                     resp.HorasFaltantes = row["HorasFaltantesFormateado"]?.ToString() ?? "0h 0min";
                 }*/

                Dictionary<string, object?>? row = totalData.FirstOrDefault();

                if (row != null)
                {
                    resp.HorasRealizadas = row["HorasRealizadasTexto"]?.ToString() ?? "0h";
                    resp.HorasRealizadasDbl = Convert.ToDouble(row["HorasRealizadasDbl"] ?? 0);
                    resp.MetaDelDia = Convert.ToDouble(row["MetaDelDia"] ?? 0);
                    resp.Porcentaje = Convert.ToInt32(row["Porcentaje"] ?? 0);
                    resp.HorasFaltantes = row["HorasFaltantesFormateado"]?.ToString() ?? "0h 0min";
                }

                return new DataResponse<ProgressHoursConfigDto>(true, resp, OK);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                throw;
            }
        }

        private async Task<UserEntity> GetUserByIdAsync(Guid userId)
        {
            UserEntity? user = await _userManager.FindByIdAsync(userId.ToString());
            return user ?? throw new NotFoundException("El usuario no fue encontrado");
        }

        private async Task<bool> ProcesarHojaCategoriasAsync(IXLWorksheet hoja)
        {
            IXLRange? rango = hoja.RangeUsed();
            if (rango == null || rango.RowCount() <= 1) return false;

            IEnumerable<IXLRangeRow>? filas = rango.RowsUsed().Skip(1);

            List<string>? categoriasEnBD = await _dbCxt.Categories
                .Where(c => !c.IsDeleted)
                .Select(c => c.Name.Trim().ToLower())
                .ToListAsync();

            HashSet<string>? nombresHash = new HashSet<string>(categoriasEnBD.Select(n => RemoveAccents(n.Trim().ToLower())));

            List<CategoriesEntity>? nuevasCategorias = new List<CategoriesEntity>();

            foreach (IXLRangeRow? fila in filas)
            {
                string? nombreRaw = fila.Cell(1).GetValue<string>()?.Trim();

                if (string.IsNullOrEmpty(nombreRaw)) continue;

                string nombreClean = nombreRaw.Trim();
                string nombreParaComparar = RemoveAccents(nombreClean.ToLower());

                if (!nombresHash.Contains(nombreParaComparar))
                {
                    nuevasCategorias.Add(new CategoriesEntity
                    {
                        Name = nombreClean,
                        Descripcion = fila.Cell(2).GetValue<string>()?.Trim(),
                        Color = fila.Cell(3).GetValue<string>()?.Trim(),
                        Slug = URLFriendly(nombreClean)
                    });

                    nombresHash.Add(nombreParaComparar);
                }
            }

            if (nuevasCategorias.Any())
            {
                try
                {
                    await _dbCxt.Categories.AddRangeAsync(nuevasCategorias);
                    await _dbCxt.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine($"Error de duplicado: {ex.InnerException?.Message}");
                    throw new Exception("Existen categorías duplicadas que no pudieron ser procesadas.");
                }
            }

            return true;
        }

        private async Task<bool> ProcesarHojaRequerimientosAsync(IXLWorksheet hoja)
        {
            IXLRange? rango = hoja.RangeUsed();
            if (rango == null || rango.RowCount() <= 1) return false;

            IEnumerable<IXLRangeRow>? filas = rango.RowsUsed().Skip(1);

            List<string>? reqsEnBD = await _dbCxt.Requeriments
                .Where(c => !c.IsDeleted && c.UserId == _userId)
                .Select(c => c.ReqID.Trim())
                .ToListAsync();

            HashSet<string>? reqsHash = new HashSet<string>(reqsEnBD);

            List<RequerimentsEntity>? nuevosRequerimientos = new List<RequerimentsEntity>();

            foreach (IXLRangeRow? fila in filas)
            {
                IXLCell? celdaNombre = fila.Cell(1);
                string reqID = celdaNombre.GetValue<string>()?.Trim() ?? string.Empty;

                if (reqID.StartsWith("ReqID"))
                {
                    reqID = reqID.Substring(5);
                }

                string urlDestino = string.Empty;

                if (celdaNombre.HasHyperlink)
                {
                    XLHyperlink? hyperlink = celdaNombre.GetHyperlink();

                    if (hyperlink.IsExternal)
                    {
                        urlDestino = hyperlink.ExternalAddress.ToString();
                    }
                    else
                    {
                        urlDestino = hyperlink.InternalAddress;
                    }
                }

                string titulo = fila.Cell(2).GetValue<string>()?.Trim() ?? string.Empty;
                string cliente = fila.Cell(3).GetValue<string>()?.Trim() ?? string.Empty;
                string storyPoint = fila.Cell(4).GetValue<string>()?.Trim() ?? string.Empty;
                string descripcion = fila.Cell(5).GetValue<string>()?.Trim() ?? string.Empty;
                string categoryName = fila.Cell(6).GetValue<string>()?.Trim() ?? string.Empty;

                CategoriesEntity category = await _dbCxt.Categories.FirstAsync(c => c.Name == categoryName);

                if (!reqsHash.Contains(reqID))
                {

                    int? folderId = null;

                    List<Guid> allowedCategoryGuids = await _dbCxt.Categories
                           .Where(c => _allowedCategoryNames.Contains(c.Name))
                           .Select(c => c.Id)
                           .ToListAsync();

                    if (allowedCategoryGuids.Contains(category.Id))
                    {
                        folderId = await FolderIdIdentity(_userId);
                    }
                    else
                    {
                        folderId = null;
                    }

                    RequerimentsEntity req = new RequerimentsEntity()
                    {
                        ReqID = reqID,
                        Titulo = titulo,
                        Cliente = cliente,
                        StoryPoint = storyPoint,
                        Url = urlDestino,
                        Descripcion = descripcion,
                        UserId = _userId,
                        FolderId = folderId,
                        CategoryId = category.Id,
                        Estado = Estados.Pendiente,
                        EtapaActual = Etapas.Alta
                    };
                
                    nuevosRequerimientos.Add(req);
                    reqsHash.Add(reqID);
                }
            }

            if (nuevosRequerimientos.Any())
            {
                try
                {
                    await _dbCxt.Requeriments.AddRangeAsync(nuevosRequerimientos);
                    await _dbCxt.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine($"Error de duplicado: {ex.InnerException?.Message}");
                    throw new Exception("Existen requerimientos duplicados que no pudieron ser procesadas.");
                }
            }


            return true;
        }

        private async Task<bool> ProcesarHojaActividadesAsync(IXLWorksheet hoja)
        {
            IXLRange? rango = hoja.RangeUsed();
            if (rango == null || rango.RowCount() <= 1) return false;

            IEnumerable<IXLRangeRow>? filas = rango.RowsUsed().Skip(1);
            List<ActivitiesEntity> nuevosActivities = new List<ActivitiesEntity>();

            List<ActivitySummaryDto>? actividadesExistentes = await _dbCxt.Activities
                .Where(a => !a.IsDeleted && a.UserId == _userId)
                .Select(a => new ActivitySummaryDto (a.RequerimentId, a.StartDate, a.StartTime, a.EndTime))
                .ToListAsync();

            foreach (IXLRangeRow? fila in filas)
            {
                string rawReqID = fila.Cell(1).GetValue<string>()?.Trim() ?? string.Empty;
                string reqID = rawReqID.StartsWith("ReqID") ? rawReqID.Substring(5) : rawReqID;

                RequerimentsEntity? req = await _dbCxt.Requeriments
                    .FirstOrDefaultAsync(r => r.ReqID == reqID && r.UserId == _userId && !r.IsDeleted);

                if (req is null) continue;

                if (!DateTime.TryParse(fila.Cell(2).GetValue<string>(), out DateTime d)) continue;
                DateOnly fechaAct = DateOnly.FromDateTime(d);

                TimeOnly.TryParse(fila.Cell(3).GetValue<string>(), out TimeOnly horaInicio);

                string descripcion = fila.Cell(5).GetValue<string>()?.Trim() ?? string.Empty;
                bool isLoaded = fila.Cell(6).GetValue<string>()?.Trim().ToLower() == "true";
                string status = fila.Cell(7).GetValue<string>()?.Trim() ?? "in-progress";

                TimeOnly? horaFinNullable = null;
                if (TimeSpan.TryParse(fila.Cell(4).GetValue<string>(), out TimeSpan et))
                {
                    horaFinNullable = TimeOnly.FromTimeSpan(et);
                }

                TimeOnly finNuevaEfectiva = horaFinNullable ?? new TimeOnly(23, 59, 59);

                bool hayTraslape = actividadesExistentes.Any(a =>
                    a.RequerimentId == req.Id &&
                    a.StartDate == fechaAct &&
                    horaInicio < (a.EndTime ?? new TimeOnly(23, 59, 59)) &&
                    finNuevaEfectiva > a.StartTime
                );

                if (!hayTraslape)
                {
                    ActivitiesEntity act = new ActivitiesEntity()
                    {
                        RequerimentId = req.Id,
                        UrlIndetificator = Generate(LowercaseLettersAndDigits, 10),
                        StartDate = fechaAct,
                        StartTime = horaInicio,
                        EndTime = finNuevaEfectiva,
                        Description = descripcion,
                        IsLoaded = isLoaded,
                        UserId = _userId,
                        StatusMessage = status
                    };

                    nuevosActivities.Add(act);

                    actividadesExistentes.Add(new ActivitySummaryDto
                    (
                        req.Id,
                        fechaAct,
                        horaInicio,
                        finNuevaEfectiva
                    ));
                }
                else
                {
                    Console.WriteLine($"Salteada: El horario {horaInicio}-{finNuevaEfectiva} para el dia {fechaAct} y para el ReqID{reqID} se solapa con uno existente.");
                }
            }

            if (nuevosActivities.Any())
            {
                try
                {
                    await _dbCxt.Activities.AddRangeAsync(nuevosActivities);
                    await _dbCxt.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine($"Error de duplicado: {ex.InnerException?.Message}");
                    throw new Exception("Existen actividades duplicadas que no pudieron ser procesadas.");
                }
            }

            return true;
        }

        private async Task<bool> ProcesarHojaCapacitacionesAsync(IXLWorksheet hoja)
        {
            IXLRange? rango = hoja.RangeUsed();
            if (rango == null || rango.RowCount() <= 1) return false;

            IEnumerable<IXLRangeRow>? filas = rango.RowsUsed().Skip(1);
            List<TrainingEntity> nuevasCapacitaciones = new List<TrainingEntity>();

            List<ActivitySummaryDto>? capacitacionesExistentes = await _dbCxt.Trainings
                .Where(a => !a.IsDeleted && a.UserId == _userId)
                .Select(a => new ActivitySummaryDto(a.RequerimentId, a.StartDate, a.StartTime, a.EndTime))
                .ToListAsync();

            foreach (IXLRangeRow fila in filas)
            {
                string rawReqID = fila.Cell(1).GetValue<string>()?.Trim() ?? string.Empty;
                string reqID = rawReqID.StartsWith("ReqID") ? rawReqID.Substring(5) : rawReqID;

                RequerimentsEntity? req = await _dbCxt.Requeriments
                    .FirstOrDefaultAsync(r => r.ReqID == reqID && r.UserId == _userId && !r.IsDeleted);

                if (req is null) continue;

                if (!DateTime.TryParse(fila.Cell(2).GetValue<string>(), out DateTime d)) continue;
                DateOnly fechaAct = DateOnly.FromDateTime(d);

                TimeOnly.TryParse(fila.Cell(3).GetValue<string>(), out TimeOnly horaInicio);

                string descripcion = fila.Cell(5).GetValue<string>()?.Trim() ?? string.Empty;
                bool isLoaded = fila.Cell(6).GetValue<string>()?.Trim().ToLower() == "true";
                string capacitator = fila.Cell(7).GetValue<string>()?.Trim() ?? string.Empty;
                string notes = fila.Cell(8).GetValue<string>()?.Trim() ?? string.Empty;
                string status = fila.Cell(9).GetValue<string>()?.Trim() ?? "in-progress";

                TimeOnly? horaFinNullable = null;
                if (TimeSpan.TryParse(fila.Cell(4).GetValue<string>(), out TimeSpan et))
                {
                    horaFinNullable = TimeOnly.FromTimeSpan(et);
                }

                TimeOnly finNuevaEfectiva = horaFinNullable ?? new TimeOnly(23, 59, 59);

                bool hayTraslape = capacitacionesExistentes.Any(a =>
                   a.RequerimentId == req.Id &&
                   a.StartDate == fechaAct &&
                   horaInicio < (a.EndTime ?? new TimeOnly(23, 59, 59)) &&
                   finNuevaEfectiva > a.StartTime
               );

                if (!hayTraslape)
                {
                    TrainingEntity training = new TrainingEntity
                    {
                        RequerimentId = req.Id,
                        StartDate = fechaAct,
                        StartTime = horaInicio,
                        EndTime = finNuevaEfectiva,
                        Capacitator = capacitator,
                        Description = descripcion,
                        IsLoaded = isLoaded,
                        Notes = notes,
                        UserId = _userId,
                        Status = status
                    };

                    nuevasCapacitaciones.Add(training);
                    capacitacionesExistentes.Add(new ActivitySummaryDto
                    (
                        req.Id,
                        fechaAct,
                        horaInicio,
                        finNuevaEfectiva
                    ));
                }
                else
                {
                    Console.WriteLine($"Salteada: El horario {horaInicio}-{finNuevaEfectiva} para el dia {fechaAct} y para el ReqID{reqID} se solapa con uno existente.");
                }                
            }

            if (nuevasCapacitaciones.Any())
            {
                try
                {
                    await _dbCxt.Trainings.AddRangeAsync(nuevasCapacitaciones);
                    await _dbCxt.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine($"Error de duplicado: {ex.InnerException?.Message}");
                    throw new Exception("Existen capacitaciones duplicadas que no pudieron ser procesadas.");
                }
            }

            return true;
        }

        private async Task<bool> ProcesarHojaRechazosAsync(IXLWorksheet hoja)
        {
            IXLRange? rango = hoja.RangeUsed();
            if (rango == null || rango.RowCount() <= 1) return false;

            List<IXLRangeRow>? filas = rango.RowsUsed().Skip(1).ToList();

            List<string>? uniqueReqIDs = filas
                .Select(f => {
                    string raw = f.Cell(1).GetValue<string>()?.Trim() ?? string.Empty;
                    return raw.StartsWith("ReqID") ? raw.Substring(5) : raw;
                })
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList();

            Dictionary<string, Guid>? requerimientosDb = await _dbCxt.Requeriments
                .Where(r => uniqueReqIDs.Contains(r.ReqID) && r.UserId == _userId && !r.IsDeleted)
                .ToDictionaryAsync(r => r.ReqID, r => r.Id);

            List<RejectionEntity>? rechazosExistentes = await _dbCxt.Rejections
                .Where(r => !r.IsDeleted && r.UserId == _userId)
                .ToListAsync();

            List<Guid>? idsRechazosInvolucrados = rechazosExistentes.Select(r => r.Id).ToList();

            List<RejectionExistDto>? detallesExistentesDb = await _dbCxt.RejectionDetails
                .Where(d => idsRechazosInvolucrados.Contains(d.RejectionId))
                .Select(d => new RejectionExistDto(d.RejectionId, d.RejectionDate, d.RejectionReason))
                .ToListAsync();

            Dictionary<string, RejectionEntity> mapaRechazos = new Dictionary<string, RejectionEntity>();

            List<RejectionEntity> nuevosRechazos = new List<RejectionEntity>();

            foreach (string? reqID in uniqueReqIDs)
            {
                if (!requerimientosDb.TryGetValue(reqID, out Guid reqGuid)) continue;

                RejectionEntity? rejPadre = rechazosExistentes.FirstOrDefault(r => r.RequerimentId == reqGuid);

                if (rejPadre == null)
                {
                    rejPadre = new RejectionEntity
                    {
                        RequerimentId = reqGuid,
                        UserId = _userId,
                        UrlIndetificator = Generate(LowercaseLettersAndDigits, 10),
                        TotalRejections = 0,
                        Status = "pending"
                    };
                    nuevosRechazos.Add(rejPadre);
                }
                mapaRechazos[reqID] = rejPadre;
            }

            if (nuevosRechazos.Any())
            {
                try
                {
                    await _dbCxt.Rejections.AddRangeAsync(nuevosRechazos);
                    await _dbCxt.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine($"Error de duplicado: {ex.InnerException?.Message}");
                    throw new Exception("Existen rechazos duplicados que no pudieron ser procesadas.");
                }
            }

            List<RejectionDetailEntity> nuevosDetalles = new List<RejectionDetailEntity>();

            Dictionary<Guid, int> contadoresRechazos = new Dictionary<Guid, int>();

            foreach (IXLRangeRow? fila in filas)
            {
                string rawReqID = fila.Cell(1).GetValue<string>()?.Trim() ?? string.Empty;
                string reqID = rawReqID.StartsWith("ReqID") ? rawReqID.Substring(5) : rawReqID;

                if (mapaRechazos.TryGetValue(reqID, out RejectionEntity? rejPadre))
                {
                    if (rejPadre.Status?.ToLower() == "completed") continue;

                    if (!DateTime.TryParse(fila.Cell(3).GetValue<string>(), out DateTime fechaRechazoDt)) continue;
                    DateOnly fechaRechazo = DateOnly.FromDateTime(fechaRechazoDt);
                    string motivo = fila.Cell(4).GetValue<string>()?.Trim() ?? string.Empty;

                    bool yaExisteEnDb = detallesExistentesDb.Any(d =>
                        d.RejectionId == rejPadre.Id &&
                        d.RejectionDate == fechaRechazo &&
                        d.RejectionReason == motivo);

                    bool yaAgregadoEnLista = nuevosDetalles.Any(d =>
                        d.RejectionId == rejPadre.Id &&
                        d.RejectionDate == fechaRechazo &&
                        d.RejectionReason == motivo);

                    if (yaExisteEnDb || yaAgregadoEnLista)
                    {
                        Console.WriteLine($"Detalle saltado por duplicado: {reqID} - {motivo}");
                        continue;
                    }

                    if (!contadoresRechazos.ContainsKey(rejPadre.Id))
                        contadoresRechazos[rejPadre.Id] = await GetMaxRechazoNro(rejPadre.Id);

                    contadoresRechazos[rejPadre.Id]++;

                    RejectionDetailEntity? detalle = new RejectionDetailEntity
                    {
                        RejectionId = rejPadre.Id,
                        UserId = _userId,
                        RejectionDate = fechaRechazo,
                        RejectionReason = motivo,
                        RejectionDetails = fila.Cell(5).GetValue<string>()?.Trim() ?? string.Empty,
                        Status = fila.Cell(10).GetValue<string>()?.Trim()?.ToLower() ?? "pending",
                        RechazoNro = contadoresRechazos[rejPadre.Id]
                    };

                    if (DateTime.TryParse(fila.Cell(6).GetValue<string>(), out DateTime fechaSol))
                        detalle.SolutionDate = DateOnly.FromDateTime(fechaSol);

                    nuevosDetalles.Add(detalle);

                    bool filaResuelta = fila.Cell(2).GetValue<string>()?.Trim().ToLower() == "true";
                    if (filaResuelta) rejPadre.IsResolve = true;
                    rejPadre.TotalRejections++;
                    _dbCxt.Entry(rejPadre).State = EntityState.Modified;
                }
            }

            if (nuevosDetalles.Any())
            {
                try
                {
                    await _dbCxt.RejectionDetails.AddRangeAsync(nuevosDetalles);
                    await _dbCxt.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine($"Error de duplicado: {ex.InnerException?.Message}");
                    throw new Exception("Existen rechazos detalles duplicados que no pudieron ser procesadas.");
                }
            }

            return true;
        }

        private async Task<int> GetMaxRechazoNro(Guid rejectionId)
        {
            return await _dbCxt.RejectionDetails
                .Where(r => r.RejectionId == rejectionId)
                .MaxAsync(r => (int?)r.RechazoNro) ?? 0;
        }

        private string RemoveAccents(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;

            return new string(text
                .Normalize(FormD)
                .Where(ch => GetUnicodeCategory(ch) != NonSpacingMark)
                .ToArray())
                .Normalize(FormC);
        }

        private async Task<int> FolderIdIdentity(Guid userId)
        {
            int maxFolderId = await _dbCxt.Requeriments
                .Where(r => r.UserId == userId && r.FolderId != null)
                .MaxAsync(r => (int?)r.FolderId) ?? 0;

            return maxFolderId + 1;
        }

        /// <summary>
        /// Recupera la configuración actual del usuario autenticado desde la base de datos.
        /// </summary>
        /// <remarks>
        /// - Carga de forma explícita las colecciones y entidades relacionadas:
        ///   <c>DayConfigs</c>, <c>NotificationConfig</c>, <c>WorkingSaturdays</c>,
        ///   <c>WeeklyPar</c> y <c>WeeklyImpar</c>.
        /// - Filtra por el <c>UserId</c> obtenido de <c>_userContext</c>, por la marca
        ///   de configuración actual (<c>ActualConfig == 1</c>) y por no estar marcada como borrada
        ///   (<c>IsDeleted == false</c>).
        /// - Si no existe una configuración actual para el usuario, se registra una advertencia
        ///   y se lanza una <see cref="KeyNotFoundException"/>.
        /// </remarks>
        /// <returns>
        /// La entidad <see cref="ConfigurationEntity"/> correspondiente a la configuración actual.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Lanzada cuando no se encuentra una configuración actual para el usuario.
        /// </exception>
        private async Task<ConfigurationEntity> GetConfigActual()
        {
            ConfigurationEntity? configs = await _dbCxt.Configurations
                .Include(dc => dc.DayConfigs)
                .Include(nc => nc.NotificationConfig)
                .Include(ws => ws.WorkingSaturdays)
                .Include(whp => whp.WeeklyPar)
                .Include(whi => whi.WeeklyImpar)
                .Include(u => u.User)
                .FirstOrDefaultAsync(c => c.UserId == _userId && c.ActualConfig == 1 && c.IsDeleted == false);

            if(configs is null)
            {
                _logger.LogWarning("No se encontró la configuración actual para el usuario {UserId}", _userId);
                throw new NotFoundException("No se encontró la configuración actual.");
            }

            return configs!;
        }

        private ConfigurationEntity MapDtoToEntity(UpdateConfigDto dto)
        {
            return new ConfigurationEntity
            {
                UserId = _userId,
                ActualConfig = 1,
                CreatedAt = DateTime.Now,

                AutoBackupEnabled = dto.BackupScheduled.AutoBackup,
                BackupFrecuencia = dto.BackupScheduled.Frecuencia,
                BackupTime = dto.BackupScheduled.Time,
                BackupRetention = dto.BackupScheduled.Retention,
                MaxBackup = dto.BackupScheduled.MaxBackup,

                WeeklyPar = new WeeklyHourConfig
                {
                    StartTime = dto.WeeklyPar.StartTime,
                    EndTime = dto.WeeklyPar.EndTime
                },
                WeeklyImpar = new WeeklyHourConfig
                {
                    StartTime = dto.WeeklyImpar.StartTime,
                    EndTime = dto.WeeklyImpar.EndTime
                },
                NotificationConfig = new NotificationConfigEntity
                {
                    EnableNotificationDiario = dto.NotificationConfig.EnableNotificationDiario,
                    EnableNotificationSemanal = dto.NotificationConfig.EnableNotificationSemanal,
                    EnableNotificationMetaAlcanzada = dto.NotificationConfig.EnableNotificationMetaAlcanzada,
                    NotificationsEmail = dto.NotificationConfig.NotificationsEmail,
                    HoraNotificacionDiaria = dto.NotificationConfig.HoraNotificacionDiaria
                },
                DayConfigs = dto.DayConfigs.Select(d => new DayConfigEntity
                {
                    Day = (int)d.Day,
                    DayName = d.DayName,
                    MinHours = d.MinHours,
                    MaxHours = d.MaxHours,
                    Enabled = d.Enabled
                }).ToList(),
                WorkingSaturdays = dto.WorkingSaturdays.Select(s => new WorkingSaturdayEntity
                {
                    Date = s.Date,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime
                }).ToList()
            };
        }

        private async Task UpdateEntityFromDto(ConfigurationEntity existingEntity, UpdateConfigDto dto)
        {
            existingEntity.AutoBackupEnabled = dto.BackupScheduled?.AutoBackup ?? false;
            existingEntity.BackupFrecuencia = dto.BackupScheduled?.Frecuencia;
            existingEntity.BackupTime = dto.BackupScheduled?.Time;
            existingEntity.BackupRetention = dto.BackupScheduled?.Retention;
            existingEntity.MaxBackup = dto.BackupScheduled?.MaxBackup ?? 0;


            if (existingEntity.WeeklyPar != null && dto.WeeklyPar != null)
            {
                existingEntity.WeeklyPar.StartTime = dto.WeeklyPar.StartTime;
                existingEntity.WeeklyPar.EndTime = dto.WeeklyPar.EndTime;
                existingEntity.WeeklyPar.ModifiedAt = DateTime.Now;
            }

            if (existingEntity.WeeklyImpar != null && dto.WeeklyImpar != null)
            {
                existingEntity.WeeklyImpar.StartTime = dto.WeeklyImpar.StartTime;
                existingEntity.WeeklyImpar.EndTime = dto.WeeklyImpar.EndTime;
                existingEntity.WeeklyImpar.ModifiedAt = DateTime.Now;
            }

            if (existingEntity.NotificationConfig != null && dto.NotificationConfig != null)
            {
                existingEntity.NotificationConfig.EnableNotificationDiario = dto.NotificationConfig.EnableNotificationDiario;
                existingEntity.NotificationConfig.EnableNotificationSemanal = dto.NotificationConfig.EnableNotificationSemanal;
                existingEntity.NotificationConfig.EnableNotificationMetaAlcanzada = dto.NotificationConfig.EnableNotificationMetaAlcanzada;
                existingEntity.NotificationConfig.NotificationsEmail = dto.NotificationConfig.NotificationsEmail;
                existingEntity.NotificationConfig.HoraNotificacionDiaria = dto.NotificationConfig.HoraNotificacionDiaria;
                existingEntity.NotificationConfig.ModifiedAt = DateTime.Now;
            }

            if (existingEntity.DayConfigs != null && dto.DayConfigs != null)
            {
                foreach (DayConfig? dtoDay in dto.DayConfigs)
                {
                    DayConfigEntity? entityDay = existingEntity.DayConfigs
                        .FirstOrDefault(d => d.Day == (int)dtoDay.Day);

                    if (entityDay != null)
                    {
                        entityDay.DayName = dtoDay.DayName;
                        entityDay.MinHours = dtoDay.MinHours;
                        entityDay.MaxHours = dtoDay.MaxHours;
                        entityDay.Enabled = dtoDay.Enabled;
                        entityDay.ModifiedAt = DateTime.Now;
                    }
                }
            }

            if (dto.WorkingSaturdayToDelete != null && dto.WorkingSaturdayToDelete.Any())
            {
                await _dbCxt.WorkingSaturdays
                    .Where(x => dto.WorkingSaturdayToDelete.Contains(x.Id))
                    .ExecuteDeleteAsync();
            }

            if (dto.WorkingSaturdays != null && dto.WorkingSaturdays.Any())
            {

                IEnumerable<WorkingSaturdayEntity>? newOnes = dto.WorkingSaturdays
                                .Where(ws => ws.Id == Guid.Empty) 
                                .Select(ws => new WorkingSaturdayEntity
                                {
                                    Date = ws.Date,
                                    StartTime = ws.StartTime,
                                    EndTime = ws.EndTime,
                                    ConfigurationEntityId = existingEntity.Id
                                });

                if (newOnes.Any())
                    await _dbCxt.WorkingSaturdays.AddRangeAsync(newOnes);
            }

            existingEntity.ModifiedAt = DateTime.Now;
        }
    }
}