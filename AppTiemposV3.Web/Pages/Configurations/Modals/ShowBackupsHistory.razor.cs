using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Configurations;
using AppTiemposV3.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using static NanoidDotNet.Nanoid;
using static NanoidDotNet.Nanoid.Alphabets;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;


namespace AppTiemposV3.Web.Pages.Configurations.Modals
{
    public partial class ShowBackupsHistory : ComponentBase
    {
        #region Variables
        public Guid Id { get; set; }
        [Inject] private IJSRuntime? JS { get; set; }
        [Inject] private ColorService ColorService { get; set; } = null!;
        [Inject] private NotificationService Toltip { get; set; } = default!;
        [Inject] private NavigationManager Nav { get; set; } = null!;
        [Inject] private IConfigurationContract ConfigurationContract { get; set; } = null!;
        [Parameter] public EventCallback<SavedEventArgs> OnSaved { get; set; }
        [Parameter] public List<AutoBackup> backups { get; set; } = null!;
        [Parameter] public int? MaxCountsBackups { get; set; } = 3;

        private bool IsDownloadingBack = false;
        private bool IsRestoreBack = false;

        private string IdModalStr = Generate(LowercaseLettersAndDigits, 10);
        private ElementReference showModalRef;
        private ElementReference closeModalRef;
        #endregion

        #region Inicializacion
        public async Task ShowAsync(Guid id)
        {
            Id = id;
            await JS!.InvokeVoidAsync("modalHelpers.clickElement", showModalRef);
            StateHasChanged();
        }
        #endregion

        #region Funciones
        private async void CloseModal()
        {
            await JS!.InvokeVoidAsync("modalHelpers.clickElement", closeModalRef);
        }

        private async Task DownloadBackup(Guid id, string fileName)
        {
            IsDownloadingBack = true;
            StateHasChanged();

            try
            {
                using Stream? stream = await ConfigurationContract.DownloadFileBackup(id);

                if (stream != null)
                {
                    using DotNetStreamReference? streamRef = new DotNetStreamReference(stream);

                    await JS!.InvokeVoidAsync("downloadFileFromStreamSQL", fileName, streamRef);

                    Toltip.Success("Éxito!", "Backup descargado correctamente.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR]: error response backend: {ex.Message}");
                throw;
            }
            finally
            {
                IsDownloadingBack = false;
                StateHasChanged();
            }
        }

        private async Task RestoreBackup(Guid id)
        {
            IsRestoreBack = true;
            StateHasChanged();
            try
            {
                GeneralResponse resp = await ConfigurationContract.RestoreBackupFromFileServer(id);

                if (resp.Flag)
                {
                   CloseModal();
                   Toltip.Success("Éxito", resp.Message);
                }
                else
                {
                    CloseModal();
                    Toltip.Error("Error", resp.Message);
                }
            }
            finally
            {
                IsRestoreBack = false;
                StateHasChanged();
            }
        }
        #endregion
    }
}
