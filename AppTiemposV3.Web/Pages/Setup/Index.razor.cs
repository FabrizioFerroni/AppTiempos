using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs.Configurations;
using AppTiemposV3.Web.Authentication;
using AppTiemposV3.Web.Services;
using Blazored.LocalStorage;
using ChartJs.Blazor.Common;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Globalization;
using System.Security.Claims;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.SharedClases.Utilidades.DateHelper;
using static AppTiemposV3.Web.Utils.Helpers;
using static System.Globalization.CultureInfo;

namespace AppTiemposV3.Web.Pages.Setup
{
    public partial class Index : ComponentBase, IDisposable
    {
        #region Variables
        #region Inyecciones
        [Inject] private NavigationManager Nav { get; set; } = null!;
        [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = null!;
        [Inject] private ColorService ColorService { get; set; } = null!;
        [Inject] private IConfigurationContract ConfigurationContract { get; set; } = null!;
        [Inject] private ILocalStorageService _localStorageService { get; set; } = default!;
        [Inject] private NotificationService Toltip { get; set; } = default!;
        #endregion

        private int currentStep { get; set; }  = 1;
        private bool isLoadingConfig { get; set; } = false;
        private DayHours WeeklyPar = new DayHours()
        {
            StartTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(0)),
            EndTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(0))
        };

        private DayHours WeeklyImpar = new DayHours()
        {
            StartTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(0)),
            EndTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(0))
        };

        private BackupScheduled backup = new();
        private NotificationConfigDto notifCfg = new();
        private bool IsSavingConfig = false;
        private List<WorkingSaturday> WorkingSaturdays = new();
        private string KeyHorasSemanales = "HorasSemanalesConfig";
        private string KeySabadosConfig = "SabadosLaborablesConfig";
        private CreateConfigurationDto newConfig = new();


        #endregion

        protected override async Task OnInitializedAsync()
        {
            ColorService.OnColorChanged += HandleColorChanged;
            
            AuthenticationState? authState = await AuthStateProvider.GetAuthenticationStateAsync();
            ClaimsPrincipal? user = authState.User;

            if (user.Identity is { IsAuthenticated: true })
            {
                await GetHasConfiguration();
            }

            await VerifyAuthState();
            await AssignDefaultDataLS();
        } 

        #region FuncionesPrincipales
        private async void HandleColorChanged()
        {
            await InvokeAsync(StateHasChanged);
        }

        private List<DayConfig> WorkHours = new()
        {
            new() { Day = DiasSemana.Lunes, DayName = "Lunes", MinHours = 0, MaxHours = 0, Enabled = false },
            new() { Day = DiasSemana.Martes, DayName = "Martes", MinHours = 0, MaxHours = 0, Enabled = false },
            new() { Day = DiasSemana.Miercoles, DayName = "Miércoles", MinHours = 0, MaxHours = 0, Enabled = false },
            new() { Day = DiasSemana.Jueves, DayName = "Jueves", MinHours = 0, MaxHours = 0, Enabled = false },
            new() { Day = DiasSemana.Viernes, DayName = "Viernes", MinHours = 0, MaxHours = 0, Enabled = false },
            new() { Day = DiasSemana.Sabado, DayName = "Sábado", MinHours = 0, MaxHours = 0, Enabled = false },
        };

        bool IsNextDisabled => WorkHours.Count(d => d.Enabled && d.MaxHours > 0 && d.MaxHours >= d.MinHours) < 5;

        private double ParseDouble(string? value)
        {
            if (double.TryParse(
                value,
                NumberStyles.Any,
                InvariantCulture,
                out double result))
            {
                return result;
            }

            return 0;
        }

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
        #endregion

        #region Funciones
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

        private void SetCurrentStep(int step)
        {
            currentStep = step;
            StateHasChanged();
        }

        public string GetScheduleDuration(DayHours schedule)
        {
            TimeSpan duration = schedule.EndTime - schedule.StartTime;

            if (duration.TotalMinutes < 0)
            {
                duration = duration.Add(TimeSpan.FromHours(24));
            }

            int hours = (int)duration.TotalHours;
            int minutes = duration.Minutes;

            return $"{hours}h {(minutes > 0 ? $"{minutes}m" : "")}".Trim();
        }

        private async Task GetHasConfiguration()
        {
            isLoadingConfig = true;
            StateHasChanged();

            try
            {
                DataResponse<bool> response = await ConfigurationContract.HasConfiguration();

                if (response.Success)
                {
                    if (response.Data)
                    {
                        Nav.NavigateTo("/app/dashboard");
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }
            finally
            {
                isLoadingConfig = false;
                StateHasChanged();
            }
        }

        private async Task CreateConfiguration()
        {
            IsSavingConfig = true;
            StateHasChanged();

            try
            {
                await ResetLocalStorage();

                newConfig = new CreateConfigurationDto
                {
                    DayConfigs = WorkHours,
                    WeeklyPar = WeeklyPar,
                    WeeklyImpar = WeeklyImpar,
                    WorkingSaturdays = WorkingSaturdays,
                    NotificationConfig = notifCfg,
                    BackupScheduled = backup
                };

                GeneralResponse response = await ConfigurationContract.CreateConfig(newConfig);

                if (response.Flag)
                {
                    Toltip.Success("Éxito!", response.Message);
                    await AssignDataLS(newConfig.DayConfigs, newConfig.WorkingSaturdays);
                    Nav.NavigateTo("/app/dashboard");
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
        #endregion


        #region Validaciones
        private async Task VerifyAuthState()
        {
            CustomAuthenticationProvider? customAuthStateProvider = (CustomAuthenticationProvider)AuthStateProvider;
            AuthenticationState? authState = await customAuthStateProvider.GetAuthenticationStateAsync();

            ClaimsPrincipal? user = authState.User;

            if (!user.Identity!.IsAuthenticated)
            {
                Nav.NavigateTo("/");
            }
        }
        #endregion

        #region Limpiar
        public void Dispose()
        {
            ColorService.OnColorChanged -= HandleColorChanged;
        }
        #endregion
    }
}
