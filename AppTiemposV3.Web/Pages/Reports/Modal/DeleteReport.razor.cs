using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static NanoidDotNet.Nanoid;
using static NanoidDotNet.Nanoid.Alphabets;

namespace AppTiemposV3.Web.Pages.Reports.Modal
{
    public partial class DeleteReport : ComponentBase
    {
        public Guid Id { get; set; }
        public Guid IdShow { get; } = Guid.NewGuid();
        public String Name { get; set; }

        [Inject] private IJSRuntime? JS { get; set; }
        [Parameter] public EventCallback OnSaved { get; set; }
        private string IdModalStr = Generate(LowercaseLettersAndDigits, 10);

        [Inject] private NotificationService Toltip { get; set; } = default!;
        [Inject] private IReportContract ReportService { get; set; } = null!;

        private ElementReference showModalRef;
        private ElementReference showModalRef2;
        private ElementReference closeModalRef;


        public async Task ShowAsync(Guid id, string name)
        {
            Id = id;
            Name = name;
            await JS!.InvokeVoidAsync("modalHelpers.clickElement", showModalRef2);
            StateHasChanged();
        }

        private async Task DeleteReportAc(Guid id)
        {
            try
            {
                await JS!.InvokeVoidAsync("modalHelpers.clickElement", showModalRef);
                StateHasChanged();
                GeneralResponse? response = await ReportService.DeleteReport(id);

                if (response.Flag)
                {
                    await JS!.InvokeVoidAsync("modalHelpers.clickElement", closeModalRef);
                    StateHasChanged();
                    Toltip.Success("Éxito!", response.Message);
                }
                else
                {
                    Toltip.Error("Algo salió mal", response.Message ?? "Hubo un error");
                }

                await OnSaved.InvokeAsync();
            }
            finally
            {
                StateHasChanged();
            }
        }
    }
}
