using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.Web.Services;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components;
using AppTiemposV3.SharedClases.DTOs.Configurations;

namespace AppTiemposV3.Web.Components.UI
{
    public partial class WorkingSaturdayBanner : ComponentBase, IDisposable
    {
        #region Variables
        [Inject] private ColorService ColorService { get; set; } = null!;
        [Inject] private ISessionStorageService _sessionStorageService { get; set; } = default!;
        [Inject] private IConfigurationContract _configurationService { get; set; } = default!;

        private readonly string key = "mostrarBanner";
        private string positionBanner { get; set; } = "hidden";
        public string SemanaWork { get; set; }
        public string DayWork { get; set; }
        public string HorasWork { get; set; }
        public double TotalHsWork { get; set; }
        #endregion

        #region Inizializacion
        protected override async Task OnInitializedAsync()
        {
            ColorService.OnColorChanged += HandleColorChanged;

            string? bannerStatus = await _sessionStorageService.GetItemAsStringAsync(key);

            if (bannerStatus == "false")
            {
                positionBanner = "hidden";
                await GetSaturdayWorks(skipVisibilityUpdate: true);
            }
            else
            {
                await GetSaturdayWorks(skipVisibilityUpdate: false);
            }

        }
        #endregion

        #region Funciones
        public async Task GetSaturdayWorks(bool skipVisibilityUpdate = false)
        {
            try
            {
                DataResponse<SaturdayBannerConfigDto> response = await _configurationService.ThisWeekHaveSaturdayWork();

                if (response.Success)
                {
                    SaturdayBannerConfigDto data = response.Data;

                    if (!skipVisibilityUpdate)
                    {
                        if (data.SaturdayWork)
                        {
                            positionBanner = "fixed";
                            await _sessionStorageService.SetItemAsStringAsync(key, "true");
                        }
                        else
                        {
                            positionBanner = "hidden";
                            await _sessionStorageService.SetItemAsStringAsync(key, "false");
                        }
                    }

                    SemanaWork = data.SemanaWork;
                    DayWork = data.DayWork;
                    HorasWork = data.HorasWork;
                    TotalHsWork = data.TotalHsWork;

                }

            }
            catch(Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                throw;
            }
        }

        private async void HandleColorChanged()
        {
            await InvokeAsync(StateHasChanged);
        }

        private async void CloseBanner()
        {
            positionBanner = "hidden";
            await _sessionStorageService.SetItemAsStringAsync(key, "false");
            StateHasChanged();
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
