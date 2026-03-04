using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs.Configurations;
using AppTiemposV3.Web.Components.UI;
using AppTiemposV3.Web.Pages.Configurations.Modals;
using AppTiemposV3.Web.Services;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.SharedClases.Utilidades.DateHelper;
using static AppTiemposV3.Web.Utils.Helpers;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using static System.Globalization.CultureInfo;
using static System.Globalization.NumberStyles;

namespace AppTiemposV3.Web.Pages.Configurations
{
    public partial class Index : ComponentBase, IDisposable
    {
        #region  Variables
        #region InyeccionDependencias
        [Inject] LayoutState State { get; set; } = null!;
        [Inject] private IJSRuntime? JS { get; set; }

        [Inject] private NavigationManager? Router { get; set; }
        [Inject] private NotificationService Toltip { get; set; } = default!;
        [Inject] private ColorService ColorService { get; set; } = null!;
        [Inject] private IConfigurationContract ConfigurationContract { get; set; } = null!;
        [Inject] private ILocalStorageService _localStorageService { get; set; } = default!;
        #endregion
        private string KeyHorasSemanales = "HorasSemanalesConfig";
        private string KeySabadosConfig = "SabadosLaborablesConfig";
        private bool IsLoadingConfig = false;
        private string tab = "work-hours";
        private bool IsSelectClosed = true;
        private bool DiaSelected = false;
        private bool SabadoConfigDelete = false;
        private bool IsSavingConfig = false;
        private List<WorkingSaturday> WorkingSaturdays = new();
        private DateOnly? NewSaturdayDate;
        private TimeSpan? NewSaturdayStart;
        private TimeSpan? NewSaturdayEnd;
        private BackupScheduled backup = new();
        private int backupsTotal = 0;
        private NotificationConfigDto notifCfg = new();
        private List<string> OptionsFrecuencia = new() { "Diario", "Semanal", "Mensual" };
        private List<Guid> WorkingSaturdayToDelete = new();
        private DayHours WeeklyPar = new DayHours()
        {
           StartTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(8)),
           EndTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(16))
        };

        private DayHours WeeklyImpar = new DayHours()
        {
            StartTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(9)),
            EndTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(18))
        };

        private bool IsNotUpdated { get; set; } = false;
        private UpdateConfigDto updateConfig = new();
        private bool IsLoadingDownloadBackup = false;
        private bool IsLoadingImportDataExcel = false;
        private bool IsLoadingRestoreBackup = false;
        private AutoBackup lastBackup = new();
        private List<AutoBackup> lastBackupAll = new();
        private bool IsLoadingLastBackup = false;
        #region Modales
        private Guid IdModal = Guid.NewGuid();
        private ShowBackupsHistory? backModalRef;
        private ResetDefaultConfig? resetModalRef;
        #endregion
        private InputFile? inputFileReference; 
        private InputFile? inputImportDataExcel;
        private bool IsDownloaded = false;
        private string textoFormato =
        @"---HOJA: Requerimientos---
ReqID  | Titulo | Cliente  | StoryPoint | Descripcion | Categoria
012345 | Test   | ROJOSOFT | 2.46       | Test 123    | Nuevo      --EJEMPLO

---HOJA: Actividades---
ReqID  | StartDate  | StartTime | EndTime | Descripcion | IsLoaded
012345 | 01/03/2026 | 09:25     | 09:55   | Test 123    | true    --EJEMPLO

---HOJA: Capacitaciones---
ReqID  | StartDate  | StartTime | EndTime | Descripcion               | IsLoaded | Capacitador  | Notes
012345 | 01/03/2026 | 09:25     | 09:55   | Descripcion de la capa    | true     | Pepito Perez | Test 456   --EJEMPLO

---HOJA: Rechazos---
ReqID  | IsResolved | RejectionDate  | RejectionReason     | RejectionDetails        | SolutionDate | SolutionDetails        | ExtimatedFixTime | ActualFixTime | Status
012345 | false      | 01/03/2026     | Razon del rechazo   | Detalles del rechazo    | 01/03/2026   | Detalle de la solucion | 00:50            | 00:35         | estado del rechazo  --EJEMPLO";


        #endregion


        #region Inicializacion
        protected override async Task OnInitializedAsync()
        {
            ColorService.OnColorChanged += HandleColorChanged;
            State.OnSidebarChanged += StateHasChanged;
            await State.InitializeAsync();
            await GetConfiguration();
            await GetManualBakcup();
            await GetAutoBackupsHistory();
            await GetTotalAutomaticBackups();
            await AssignDefaultDataLS();
            StateHasChanged();
        }
        #endregion

        #region FuncionesPrincipales
        private void HandleSidebarToggle()
        {
            _ = State.ToggleSidebar();
        }

        private async void HandleColorChanged()
        {
            await InvokeAsync(StateHasChanged);
        }

        private void HandleDropdownState(bool closed)
        {
            IsSelectClosed = closed;
        }
        #endregion

        #region Funciones
        public string GetScheduleDuration(DayHours schedule)
        {
            // Al restar dos TimeOnly, obtenemos un TimeSpan
            TimeSpan duration = schedule.EndTime - schedule.StartTime;

            // Si existe la posibilidad de que el horario termine al día siguiente:
            if (duration.TotalMinutes < 0)
            {
                duration = duration.Add(TimeSpan.FromHours(24));
            }

            int hours = (int)duration.TotalHours;
            int minutes = duration.Minutes;

            return $"{hours}h {(minutes > 0 ? $"{minutes}m" : "")}".Trim();
        }

        private List<DayConfig> WorkHours = new()
        {
            new() { Day = DiasSemana.Lunes, DayName = "Lunes", MinHours = 7, MaxHours = 8, Enabled = true },
            new() { Day = DiasSemana.Martes, DayName = "Martes", MinHours = 7, MaxHours = 8, Enabled = true },
            new() { Day = DiasSemana.Miercoles, DayName = "Miércoles", MinHours = 7, MaxHours = 8, Enabled = true },
            new() { Day = DiasSemana.Jueves, DayName = "Jueves", MinHours = 7, MaxHours = 8, Enabled = true },
            new() { Day = DiasSemana.Viernes, DayName = "Viernes", MinHours = 7, MaxHours = 8, Enabled = true },
            new() { Day = DiasSemana.Sabado, DayName = "Sábado", MinHours = 3.5, MaxHours = 4, Enabled = false },
        };

        private string GetIndicatorColor(DayConfig day)
        {
            double diff = day.MaxHours - day.MinHours;

            if (day.Day == DiasSemana.Sabado)
            {
                if (diff == 0)
                    return "w-3 h-3 rounded-full bg-red-500";

                if (diff < 0.5 || diff > 0.5)
                    return "w-3 h-3 rounded-full bg-yellow-500";

                return "w-3 h-3 rounded-full bg-green-500";
            }

            // Resto de los días
            if (diff == 0)
                return "w-3 h-3 rounded-full bg-red-500";

            if (diff < 1 || diff > 1)
                return "w-3 h-3 rounded-full bg-yellow-500";

            return "w-3 h-3 rounded-full bg-green-500";
        }

        private void HandleBackupCheck(bool check)
        {
            backup.AutoBackup = check;
            StateHasChanged();
        }

        private Task OnFrecuencySelectedChanged(string value)
        {
            backup.Frecuencia = value;
            return Task.CompletedTask;
        }

        private void OnDayEnabledChanged(DayConfig day, bool value)
        {
            day.Enabled = value;

            // lógica extra
            if (!value)
            {
                day.MinHours = 0;
                day.MaxHours = 0;
            }
        }

        private double GetTotalWeeklyHoursWithSaturdays()
        {
            return GetTotalWeeklyHours()
                 + WorkingSaturdays.Sum(s => s.Hours);
        }

        private double GetTotalWeeklyHours()
        {
            return WorkHours
                .Where(d => d.Enabled)
                .Sum(d => d.MaxHours);
        }

        private int GetAverageDailyHours()
        {
            int enabledDays = WorkHours.Count(d => d.Enabled);
            int saturdayFactor = WorkingSaturdays.Count > 0 ? 1 : 0;

            int divisor = enabledDays + saturdayFactor;

            if (divisor == 0)
                return 0;

            double totalHours = GetTotalWeeklyHoursWithSaturdays();

            return (int)Math.Round(totalHours / divisor);
        }

        private double ParseDouble(string? value)
        {
            if (double.TryParse(
                value,
                Any,
                InvariantCulture,
                out double result))
            {
                return result;
            }

            return 0;
        }

        private int ParseInt(string? value)
        {
            if (int.TryParse(
                value,
                Any,
                InvariantCulture,
                out int result))
            {
                return result;
            }

            return 0;
        }

        private void OnMaxHoursChanged(DayConfig day, string value)
        {
            day.MaxHours = Convert.ToDouble(value);

            if (day.MaxHours < day.MinHours)
                day.MaxHours = day.MinHours;
        }

        private void AddWorkingSaturday()
        {
            if (NewSaturdayDate is null || NewSaturdayStart is null || NewSaturdayEnd is null)
                return;

            if (NewSaturdayEnd <= NewSaturdayStart)
                return;

            // Evitar duplicados
            if (WorkingSaturdays.Any(s => s.Date == NewSaturdayDate))
                return;

            WorkingSaturdays.Add(new WorkingSaturday
            {
                Date = NewSaturdayDate.Value,
                StartTime = NewSaturdayStart.Value,
                EndTime = NewSaturdayEnd.Value
            });

            // Reset inputs
            NewSaturdayDate = null;
            NewSaturdayStart = null;
            NewSaturdayEnd = null;
        }

        private void RemoveWorkingSaturday(WorkingSaturday workSat)
        {
            WorkingSaturdayToDelete.Add(workSat.Id);
            WorkingSaturdays.Remove(workSat);
            StateHasChanged();
        }

        private async Task HandleConfigDeleted()
        {
            await GetConfiguration();
        }

        private async Task GetConfiguration()
        {
            IsLoadingConfig = true;
            StateHasChanged();

            try
            {
                DataResponse<ListActualConfig> response = await ConfigurationContract.GetConfiguration();

                ListActualConfig actualCfg = response.Data;

                IsNotUpdated = actualCfg.IsNotUpdated;

                WorkHours = actualCfg.DayConfigs.Select(d => new DayConfig
                {
                    Day = d.Day,
                    DayName = d.DayName,
                    MinHours = d.MinHours,
                    MaxHours = d.MaxHours,
                    Enabled = d.Enabled
                })
                .ToList();

                WeeklyImpar.StartTime = actualCfg.WeeklyImpar.StartTime;
                WeeklyImpar.EndTime = actualCfg.WeeklyImpar.EndTime;
                WeeklyPar.StartTime = actualCfg.WeeklyPar.StartTime;
                WeeklyPar.EndTime = actualCfg.WeeklyPar.EndTime;

                WorkingSaturdays = actualCfg.WorkingSaturdays.Select(nt => new WorkingSaturday
                {
                    Id = nt.Id,
                    Date = nt.Date,
                    StartTime = nt.StartTime,
                    EndTime = nt.EndTime
                }).ToList();

                backup = new BackupScheduled
                {
                    AutoBackup = actualCfg.BackupScheduled.AutoBackup,
                    Frecuencia = actualCfg.BackupScheduled.Frecuencia,
                    Time = actualCfg.BackupScheduled.Time,
                    Retention = actualCfg.BackupScheduled.Retention,
                    MaxBackup = actualCfg.BackupScheduled.MaxBackup
                };

                notifCfg = new NotificationConfigDto
                {
                    EnableNotificationDiario = actualCfg.NotificationConfig.EnableNotificationDiario,
                    EnableNotificationSemanal = actualCfg.NotificationConfig.EnableNotificationSemanal,
                    EnableNotificationMetaAlcanzada = actualCfg.NotificationConfig.EnableNotificationMetaAlcanzada,
                    NotificationsEmail = actualCfg.NotificationConfig.NotificationsEmail,
                    HoraNotificacionDiaria = actualCfg.NotificationConfig.HoraNotificacionDiaria,
                };

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            finally
            {
                IsLoadingConfig = false;
                StateHasChanged();
            }
        }

        /*private DiasSemana ParseDay(string day)
        {
            return Parse<DiasSemana>(
                InvariantCulture.TextInfo.ToTitleCase(day),
                ignoreCase: true
            );
        }*/

        private async Task UpdateConfiguration()
        {
            IsSavingConfig = true;
            StateHasChanged();

            try
            {
                await ResetLocalStorage();

                updateConfig = new UpdateConfigDto
                {
                    DayConfigs = WorkHours,
                    WeeklyPar = WeeklyPar,
                    WeeklyImpar = WeeklyImpar,
                    WorkingSaturdays = WorkingSaturdays,
                    NotificationConfig = notifCfg,
                    BackupScheduled = backup,
                    WorkingSaturdayToDelete = WorkingSaturdayToDelete
                };

                GeneralResponse response = await ConfigurationContract.UpdateConfig(updateConfig);

                if (response.Flag)
                {
                    Toltip.Success("Éxito!", response.Message);
                    await GetConfiguration();
                    await GetTotalAutomaticBackups();
                    await AssignDataLS(updateConfig.DayConfigs, updateConfig.WorkingSaturdays);
                }
                else
                {
                    Toltip.Error("Hubo un error", response.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            finally
            {
                IsSavingConfig = false;
                WorkingSaturdayToDelete.Clear();
                StateHasChanged();
            }
        }

        private async Task DownloadBackup()
        {
            IsLoadingDownloadBackup = true;
            StateHasChanged();
            try
            {
                using Stream? stream = await ConfigurationContract.DownloadBackup();
                if (stream != null)
                {
                    string fileName = $"backup_{DateTime.Now:dd-MM-yyyy-HH-mm}.sql";

                    using DotNetStreamReference? streamRef = new DotNetStreamReference(stream);

                    await JS!.InvokeVoidAsync("downloadFileFromStreamSQL", fileName, streamRef);

                    Toltip.Success("Éxito!", "Backup descargado correctamente.");

                    await GetManualBakcup();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR]: error response backend: {ex.Message}");
                throw;
            }
            finally
            {
                IsLoadingDownloadBackup = false;
                StateHasChanged();
            }
        }

        private async Task GetManualBakcup()
        {
            IsLoadingLastBackup = true;
            StateHasChanged();

            try
            {
                DataResponse<AutoBackup> response = await ConfigurationContract.GetLastManualBackup();
                lastBackup = response.Data;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR]: error response backend: {ex.Message}");
                throw;
            }
            finally
            {
                IsLoadingLastBackup = false;
                StateHasChanged();
            }
        }

        private async Task GetAutoBackupsHistory()
        {
            try
            {
                DataResponse<List<AutoBackup>> response = await ConfigurationContract.GetAutoBackupsHistory();
                lastBackupAll = response.Data;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR]: error response backend: {ex.Message}");
                throw;
            }
            finally
            {
                StateHasChanged();
            }
        }

        private async Task GetTotalAutomaticBackups()
        {
            try
            {
                DataResponse<int> response = await ConfigurationContract.GetTotalAutomaticBackups();
                backupsTotal = response.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR]: error response backend: {ex.Message}");
                throw;
            }
            finally
            {
                StateHasChanged();
            }
        }

        private async Task TriggerFilePickerAsync()
        {
            await JS!.InvokeVoidAsync("clickElementById", "restore-backup-input");
        }

        private async Task TriggerFilePickerExcelAsync()
        {
            await JS!.InvokeVoidAsync("clickElementById", "import-data-excel-input");
        }

        

        private async Task HandleFileSelected(InputFileChangeEventArgs e)
        {
            IsLoadingRestoreBackup = true;
            StateHasChanged();

            IBrowserFile? file = e.File;

            if (file == null) return;

            try
            {
                byte[]? buffer = new byte[file.Size];
                using Stream? stream = file.OpenReadStream(maxAllowedSize: 1024 * 1024 * 20); 
                await stream.ReadAsync(buffer);

                GeneralResponse? response = await ConfigurationContract.RestoreFromUpload(buffer, file.Name);

                if (response.Flag)
                {
                    Toltip.Success("Éxito!", response.Message);
                    IsLoadingRestoreBackup = false;
                    StateHasChanged();
                    await GetConfiguration();
                    await GetTotalAutomaticBackups();
                }
                else
                {
                    Toltip.Error("Hubo un error", response.Message);
                    IsLoadingRestoreBackup = false;
                    StateHasChanged();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR]: {ex.Message}");
            }
            finally
            {
                IsLoadingRestoreBackup = false;
                StateHasChanged();
            }
        }


        private async Task HandleImportDataExcel(InputFileChangeEventArgs e)
        {
            IsLoadingImportDataExcel = true;
            StateHasChanged();

            IBrowserFile? file = e.File;

            if (file == null) return;

            try
            {
                byte[]? buffer = new byte[file.Size];
                using Stream? stream = file.OpenReadStream(maxAllowedSize: 1024 * 1024 * 20);
                await stream.ReadAsync(buffer);

                GeneralResponse? response = await ConfigurationContract.ImportDataFromExcel(buffer, file.Name);

                if (response.Flag)
                {
                    Toltip.Success("Éxito!", response.Message);
                    IsLoadingImportDataExcel = false;
                    StateHasChanged();
                    await GetConfiguration();
                    await GetTotalAutomaticBackups();
                }
                else
                {
                    Toltip.Error("Hubo un error", response.Message);
                    IsLoadingImportDataExcel = false;
                    StateHasChanged();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR]: {ex.Message}");
            }
            finally
            {
                IsLoadingImportDataExcel = false;
                StateHasChanged();
            }
        }

        private async Task AssignDefaultDataLS()
        {
            try
            {
                string? horasSemanales = await _localStorageService.GetItemAsStringAsync(KeyHorasSemanales);
                string? sabadosConfig = await _localStorageService.GetItemAsStringAsync(KeySabadosConfig);

                if (string.IsNullOrWhiteSpace(horasSemanales))
                {
                    await _localStorageService.SetItemAsync<List<DayConfig>>(KeyHorasSemanales, new List<DayConfig>());
                }

                if (string.IsNullOrWhiteSpace(sabadosConfig))
                {
                    await _localStorageService.SetItemAsync<List<WorkingSaturday>>(KeySabadosConfig, new List<WorkingSaturday>());
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR]: {ex.Message}");
            }
            finally
            {
                StateHasChanged();
            }
        }

        private async Task AssignDataLS(List<DayConfig> dataHorasSemanales, List<WorkingSaturday> dataSabadosConfig)
        {
            try
            {
                List<Dictionary<string, string>> hoursWeekly = new();

                foreach (DayConfig day in dataHorasSemanales.Where(hs => hs.Enabled == true))
                {
                    hoursWeekly.Add(new Dictionary<string, string>
                    {
                        { "day", ((int)day.Day).ToString() },
                        { "hourMin", day.MinHours.ToString() },
                        { "hourMax", day.MaxHours.ToString() }
                    });
                }

                await _localStorageService.SetItemAsync<List<Dictionary<string, string>>>(KeyHorasSemanales, hoursWeekly);

                List<Dictionary<string, DateOnly>> dateSaturdays = new();

                foreach (WorkingSaturday? saturday in dataSabadosConfig)
                {
                    dateSaturdays.Add(new Dictionary<string, DateOnly> { { "date", saturday.Date } });
                }

                await _localStorageService.SetItemAsync<List<Dictionary<string, DateOnly>>>(KeySabadosConfig, dateSaturdays);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR]: {ex.Message}");
            }
            finally
            {
                StateHasChanged();
            }
        }

        private async Task ResetLocalStorage()
        {
            try
            {
                string? horasSemanales = await _localStorageService.GetItemAsStringAsync(KeyHorasSemanales);
                string? sabadosConfig = await _localStorageService.GetItemAsStringAsync(KeySabadosConfig);

                if (!string.IsNullOrWhiteSpace(horasSemanales))
                {
                    await _localStorageService.RemoveItemAsync(KeyHorasSemanales);
                }

                if (!string.IsNullOrWhiteSpace(sabadosConfig))
                {
                    await _localStorageService.RemoveItemAsync(KeyHorasSemanales);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR]: {ex.Message}");
            }
            finally
            {
                StateHasChanged();
            }
        }

        private async Task DescargarExcel()
        {
            // La ruta es relativa a la raíz de wwwroot
            string? fileUrl = "data/Plantilla.xlsx";
            string? fileName = "Plantilla-importacion-masiva.xlsx";

            await JS!.InvokeVoidAsync("downloadFileFromUrl", fileName, fileUrl);

            IsDownloaded = true;
            StateHasChanged();
        }
        #endregion

        #region Limpiar
        public void Dispose()
        {
            ColorService.OnColorChanged -= HandleColorChanged;
            State.OnSidebarChanged -= StateHasChanged;
        }
        #endregion

        #region ModalesFunciones
        private async Task ShowHistoryModal()
        {
            await backModalRef!.ShowAsync(IdModal);
        }

        private async Task ResetConfigModal()
        {
            await resetModalRef!.ShowAsync();
        }
        #endregion
    }
}
