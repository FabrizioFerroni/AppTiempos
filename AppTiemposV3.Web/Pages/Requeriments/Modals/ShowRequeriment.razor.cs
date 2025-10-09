using System.Globalization;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs.Categories;
using AppTiemposV3.SharedClases.DTOs.Requeriments;
using AppTiemposV3.SharedClases.Enums;
using AppTiemposV3.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using static NanoidDotNet.Nanoid;
using static NanoidDotNet.Nanoid.Alphabets;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
namespace AppTiemposV3.Web.Pages.Requeriments.Modals;

public partial class ShowRequeriment : ComponentBase
{
    #region "Variables" 
    public Guid Id { get; set; }
    [Inject] private IJSRuntime? JS { get; set; }
    
    [Inject] private NavigationManager? Router { get; set; }
    [Inject] private NotificationService Toltip { get; set; } = default!;
    [Inject] private IRequerimentContract<RequerimentResponseDto> RequerimentsService { get; set; } = null!;
    [Inject] private ColorService ColorService { get; set; } = null!;
    
    private RequerimentResponseDto requerimentShow = new RequerimentResponseDto();
    private Dictionary<string, (string Mensaje, bool EsExitoso)> mensajes = new();
    
    private bool IsLoadingData = true;
    private string IdModalStr = Generate(LowercaseLettersAndDigits, 10);
    
    private ElementReference showModalRef;
    private ElementReference closeModalRef;
    private int rechazosCount = 0; //TODO: Esto despues va a ser un contador que viene de la base de datos ...
    private int documentsFiles = 0;
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
            DataResponse<RequerimentResponseDto>? requeriment =
                await RequerimentsService.GetRequerimentporId(id);

            requerimentShow = requeriment.Data;
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
    private async void OpenCrmUrl(string url)
    {
        await JS!.InvokeVoidAsync("open", url, "_blank");
    }

    private string ObtainTimeEstimated(string storyPointsStr, Areas area)
    {
        double storyPoints = Double.Parse(storyPointsStr, CultureInfo.InvariantCulture);
        double porcentaje = area == Areas.Desarrollo ? 0.8 : 1;
        
        double horasEstimadas = storyPoints * porcentaje;
        int horas = (int)horasEstimadas;
        int minutos = (int)Math.Round((horasEstimadas - horas) * 60);

        return horas.ToString("00") + ":" + minutos.ToString("00");
    }

    private async Task  CopyToClipboard(string copy, string label, string id)
    {
        bool resultado = await JS!.InvokeAsync<bool>("copyToClipboard", copy);

        if (resultado)
        {
            mensajes[id] = ($"{label}: Texto copiado al portapapeles", true);
        }
        else
        {
            mensajes[id] = ($"Error al copiar el texto de {label} al portapapeles", false);
        }
        
        StateHasChanged(); 

        await Task.Delay(10000);

        mensajes.Remove(id);
        StateHasChanged(); 
    }
}