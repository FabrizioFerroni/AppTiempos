using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.Web.Services;
using ChartJs.Blazor.Common;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static NanoidDotNet.Nanoid;
using static NanoidDotNet.Nanoid.Alphabets;

namespace AppTiemposV3.Web.Pages.Configurations.Modals
{
    public partial class ResetDefaultConfig : ComponentBase
    {
        #region Variables
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid IdShow { get; } = Guid.NewGuid();
        [Inject] private IJSRuntime? JS { get; set; }
        [Inject] private ColorService ColorService { get; set; } = null!;
        [Inject] private IConfigurationContract ConfigurationContract { get; set; } = null!;
        [Inject] private NotificationService Toltip { get; set; } = default!;
        [Parameter] public EventCallback OnSaved { get; set; }

        private string IdModalStr = Generate(LowercaseLettersAndDigits, 10);
        private ElementReference showModalRef;
        private ElementReference showModalRef2;
        private ElementReference closeModalRef;
        #endregion

        #region Inicializacion
        public async Task ShowAsync()
        {
            await JS!.InvokeVoidAsync("modalHelpers.clickElement", showModalRef2);
            StateHasChanged();
        }
        #endregion

        #region FuncionEliminar
        private async Task ResetConfig()
        {
            try
            {
                await JS!.InvokeVoidAsync("modalHelpers.clickElement", showModalRef);
                StateHasChanged();
                GeneralResponse response = await ConfigurationContract.ResetConfig();

                if (response.Flag)
                {
                    await JS!.InvokeVoidAsync("modalHelpers.clickElement", closeModalRef);
                    StateHasChanged();
                    Toltip.Success("Éxito!", response.Message);
                }
                else
                {
                    await JS!.InvokeVoidAsync("modalHelpers.clickElement", closeModalRef);
                    StateHasChanged();
                    Toltip.Error("Algo salió mal", response.Message ?? "Hubo un error");
                }

                await OnSaved.InvokeAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            finally
            {
                StateHasChanged();
            }
        }
        #endregion
    }
}
