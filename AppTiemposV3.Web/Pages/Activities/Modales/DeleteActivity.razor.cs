using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs.Activities;
using AppTiemposV3.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using static NanoidDotNet.Nanoid;
using static NanoidDotNet.Nanoid.Alphabets;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;


namespace AppTiemposV3.Web.Pages.Activities.Modales;

public partial class DeleteActivity : ComponentBase
{
    #region Variables
    public Guid Id { get; set; }
    public Guid IdShow { get; } = Guid.NewGuid();
    public String ReqID { get; set; }
    
    public DateOnly StartDate { get; set; }
    
    [Inject] private IJSRuntime? JS { get; set; }
    [Parameter] public EventCallback<DateOnly> OnSaved { get; set; }
    private string IdModalStr = Generate(LowercaseLettersAndDigits, 10);
    
    [Inject] private NotificationService Toltip { get; set; } = default!;
    [Inject] private IActivityContract<ActivityResponseDto>  ActivityService { get; set; } = null!;
    
    private ElementReference showModalRef;
    private ElementReference showModalRef2;
    private ElementReference closeModalRef;
    #endregion
    
    #region Inicializacion
    public async Task ShowAsync(Guid id, string reqId, DateOnly startDate)
    {
        Id = id;
        ReqID = reqId;
        StartDate = startDate;
        await JS!.InvokeVoidAsync("modalHelpers.clickElement", showModalRef2);
        StateHasChanged();
    }
    #endregion
    
    #region Funciones
    private async Task DeleteActivityAc(Guid id)
    {
        try
        {
            await JS!.InvokeVoidAsync("modalHelpers.clickElement", showModalRef);
            StateHasChanged();
            GeneralResponse? response = await ActivityService.DeleteActivity(id);

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
            await OnSaved.InvokeAsync(StartDate);
        }
        finally
        {
            StateHasChanged();
        }
    }
    #endregion
}