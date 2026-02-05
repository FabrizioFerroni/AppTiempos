using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs.Activities;
using AppTiemposV3.SharedClases.DTOs.Reports;
using AppTiemposV3.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.Web.Utils.Helpers;
using static AppTiemposV3.SharedClases.Utilidades.GenerateSlug;

namespace AppTiemposV3.Web.Pages.Reports.Details
{
    public partial class Index : ComponentBase, IDisposable
    {
        #region Variables
        #region InyeccionDependencias
        [Parameter] public string urlIdentificator { get; set; } = string.Empty;
        [Inject] LayoutState State { get; set; } = null!;
        [Inject] private IJSRuntime? JS { get; set; }
        [Inject] private ColorService ColorService { get; set; } = null!;
        [Inject] private NotificationService Toltip { get; set; } = default!;
        [Inject] private IReportContract ReportService { get; set; } = null!;
        [Inject] private NavigationManager Nav { get; set; } = null!;
        #endregion
        private bool IsLoadingData { get; set; } = false;
        private bool IsLoadingPDF { get; set; } = false;
        private bool IsLoadingExcel { get; set; } = false;
        private ListReportDto Report { get; set; } = default!;
        #endregion

        #region Inicializacion
        protected async override Task OnInitializedAsync()
        {
            ColorService.OnColorChanged += HandleColorChanged;
            State.OnSidebarChanged += StateHasChanged;
            await State.InitializeAsync();
            await GetReport();
        }
        #endregion


        #region Funciones
        private async void HandleColorChanged()
        {
            await InvokeAsync(StateHasChanged);
        }
        private void HandleSidebarToggle()
        {
            _ = State.ToggleSidebar();
        }

        private async Task GetReport()
        {
            IsLoadingData = true;
            StateHasChanged();
            try
            {
                DataResponse<ListReportDto> report = await ReportService.GetReportByUrl(urlIdentificator);

                if (!report.Success)
                {
                    Nav.NavigateTo("/app/reportes");
                }

                Report = report.Data;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                IsLoadingData = false;
                StateHasChanged();
            }
        }

        private async Task DownloadPdfReport(Guid id)
        {
            IsLoadingPDF = true;
            StateHasChanged();
            try
            {
                byte[]? response = await ReportService.GeneratePDF(id);

                if (response != null)
                {
                    string? base64 = Convert.ToBase64String(response);
                    string? now = DateTime.Now.ToString("dd-MM-yyyy-HH-mm");
                    await JS!.InvokeVoidAsync("downloadFileFromStream", $"{URLFriendly(Report.Name)}-{now}.pdf", "application/pdf", base64);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR]: error response backend: {ex.Message}");
                throw;
            }
            finally
            {
                IsLoadingPDF = false;
                StateHasChanged();
            }
        }
        
        private async Task DownloadExcelReport(Guid id)
        {
            IsLoadingExcel = true;
            StateHasChanged();
            try
            {
                byte[]? response = await ReportService.GenerateExcel(id);
                if (response != null)
                {
                    string? base64 = Convert.ToBase64String(response);
                    string? now = DateTime.Now.ToString("dd-MM-yyyy-HH-mm");
                    await JS!.InvokeVoidAsync("downloadFileFromStream", $"{URLFriendly(Report.Name)}-{now}.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", base64);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR]: error response backend: {ex.Message}");
                throw;
            }
            finally
            {
                IsLoadingExcel = false;
                StateHasChanged();
            }
        }
        #endregion


        #region Limpiar
        public void Dispose()
        {
            ColorService.OnColorChanged -= HandleColorChanged;
            State.OnSidebarChanged -= StateHasChanged;
        }
        #endregion
    }
}
