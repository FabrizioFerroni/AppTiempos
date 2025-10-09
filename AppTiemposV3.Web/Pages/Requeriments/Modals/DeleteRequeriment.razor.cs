using Microsoft.AspNetCore.Components;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs.Categories;
using AppTiemposV3.SharedClases.DTOs.Requeriments;
using AppTiemposV3.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using static NanoidDotNet.Nanoid;
using static NanoidDotNet.Nanoid.Alphabets;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
namespace AppTiemposV3.Web.Pages.Requeriments.Modals;

public partial class DeleteRequeriment : ComponentBase
{
    public Guid Id { get; set; }
    public Guid IdShow { get; } = Guid.NewGuid();
    public String ReqID { get; set; }
    
    [Inject] private IJSRuntime? JS { get; set; }
    [Parameter] public EventCallback OnSaved { get; set; }
    private string IdModalStr = Generate(LowercaseLettersAndDigits, 10);
    
    [Inject] private NotificationService Toltip { get; set; } = default!;
    [Inject] private IRequerimentContract<RequerimentResponseDto> RequerimentsService { get; set; } = null!;
    
    private ElementReference showModalRef;
    private ElementReference showModalRef2;
    private ElementReference closeModalRef;
    
    
    public async Task ShowAsync(Guid id, string reqId)
    {
        Id = id;
        ReqID = reqId;
        await JS!.InvokeVoidAsync("modalHelpers.clickElement", showModalRef2);
        StateHasChanged();
    }
    
    private async Task DeleteRequerimentAc(Guid id)
    {
        try
        {
            await JS!.InvokeVoidAsync("modalHelpers.clickElement", showModalRef);
            StateHasChanged();
            GeneralResponse? response = await RequerimentsService.DeleteRequeriment(id);

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

            // Actualizar lista
            await OnSaved.InvokeAsync();
        }
        finally
        {
            StateHasChanged();
        }
    }
}