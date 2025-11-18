using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs.Requeriments;
using AppTiemposV3.SharedClases.DTOs.Trainings;
using AppTiemposV3.Web.Pages.Requeriments.Modals;
using AppTiemposV3.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using static NanoidDotNet.Nanoid;
using static NanoidDotNet.Nanoid.Alphabets;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.Web.Pages.Trainings.Modales;

public partial class ShowTrainingModal : ComponentBase
{
    #region "Variables" 
    public Guid Id { get; set; }
    [Inject] private IJSRuntime? JS { get; set; }
    
    [Inject] private NavigationManager? Router { get; set; }
    [Inject] private NotificationService Toltip { get; set; } = default!;
    [Inject] private ITrainingContract<TrainingResponseDto> TrainingService { get; set; } = null!;
    [Inject] private ColorService ColorService { get; set; } = null!;
    
    private TrainingResponseDto trainingShow = new TrainingResponseDto();
    private Dictionary<string, (string Mensaje, bool EsExitoso)> mensajes = new();
    
    private bool IsLoadingData = true;
    private string IdModalStr = Generate(LowercaseLettersAndDigits, 10);
    
    private ElementReference showModalRef;
    private ElementReference closeModalRef;
    
    #region Modales
    private ShowRequeriment? showRModalRef;
    #endregion
    #endregion

    
    public async Task ShowAsync(Guid id)
    {
        Id = id;
        await JS!.InvokeVoidAsync("modalHelpers.clickElement", showModalRef);
        IsLoadingData = true;
        StateHasChanged();
        await LoadDataReq(id);
    }
    
    private async Task LoadDataReq(Guid id)
    {
        IsLoadingData = true;
        try
        {
            DataResponse<TrainingResponseDto>? training =
                await TrainingService.GetTrainingPorId(id);

            trainingShow = training.Data;
            mensajes = new Dictionary<string, (string Mensaje, bool EsExitoso)>();
            StateHasChanged();

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
    
    private string GetStatusNameR(string status)
    {
        switch (status) {
            case "completed":
                return "Completado";
            case "in-progress":
                return "En Progreso";
            case "pending":
                return "Pendiente";
            default:
                return status;
        }
    }
    
    private string GetStatusColor(string status)
    {
        switch (status)
        {
            case "completed":
                return "bg-green-100 dark:bg-green-900/30 text-green-800 dark:text-green-200";
            case "in-progress":
                return "bg-yellow-100 dark:bg-yellow-900/30 text-yellow-800 dark:text-yellow-200";
            case "pending":
                return "bg-gray-100 dark:bg-gray-700/50 text-gray-800 dark:text-gray-200"; 
            default:
                return "";
        }
    }
    
    private async Task ShowModal(Guid id)
    {
        await showRModalRef!.ShowAsync(id);
    }

}