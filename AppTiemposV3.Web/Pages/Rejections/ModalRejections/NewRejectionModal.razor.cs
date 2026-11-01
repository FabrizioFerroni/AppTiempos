using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Rejections;
using AppTiemposV3.SharedClases.DTOs.Requeriments;
using AppTiemposV3.SharedClases.DTOs.Testers;
using AppTiemposV3.Web.Services;
using static AppTiemposV3.Web.Utils.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static NanoidDotNet.Nanoid;
using static NanoidDotNet.Nanoid.Alphabets;

namespace AppTiemposV3.Web.Pages.Rejections.ModalRejections;

public partial class NewRejectionModal : ComponentBase
{
    #region "Variables" 
    public Guid Id { get; set; }
    [Inject] private IJSRuntime? JS { get; set; }
    
    [Inject] private NavigationManager? Router { get; set; }
    [Inject] private NotificationService Toltip { get; set; } = default!;
    [Inject] private IRejectionContract<RejectionResponseDto> RejectionService { get; set; } = null!;
    [Inject] private ITesterContract<TesterResponseDto> TesterService { get; set; } = null!;
    [Inject] private ColorService ColorService { get; set; } = null!;
    private Guid RejectionId { get; set; }
    
    private Dictionary<string, (string Mensaje, bool EsExitoso)> mensajes = new();
    
    public bool IsLoadingData { get; set; } = true;
    private string IdModalStr = Generate(LowercaseLettersAndDigits, 10);
    
    
    private ElementReference showModalRef;
    private ElementReference closeModalRef;
    
    private bool isError = false;
    private MarkupString messageError = new("");

    private bool isSuccessReq;
    private MarkupString? messageSuccessReq = new("");
    private bool IsPastDateTime { get; set; } = false;
    
    private CreateRejectionDto createDto = new()
    {
        RequerimentId = default,
        TesterId = default
    };
    private bool IsLoadingNew = false;
    [Parameter] public EventCallback<SavedEventArgs> OnSaved { get; set; }
    private string ReqId {get; set;} = string.Empty;
    
    private bool IsSelectClosed = true;
    
    #region Requeriments
    private List<string> OptionsRequeriments = new() {};
    private bool IsRequerimentsNotLoaded { get; set; } = true;
    
    private string? RequerimientoSeleccionado = "";
    private string? DescripcionRequerimientoSeleccionado = "";
    private bool IsRequerimentIdSelected = false;
    private bool IsNotRequerimentIdSelectedF = true;
    private bool IsSelectClosedRequeriment = true;
    [Inject] private IRequerimentContract<RequerimentResponseDto> RequerimentsService { get; set; } = null!;
    List<RequerimentResponseDto> Requeriments = new();
    
    private string PlaceholderText
    {
        get
        {
            return IsRequerimentsNotLoaded 
                ? "Cargando requerimientos..." 
                : "Selecciona un requerimiento";
        }
    }
    private bool IsSelectClosedTester = true;

    private string ClassSelect => "font-medium bg-white dark:bg-gray-700 border-gray-300 dark:border-gray-600 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 dark:hover:bg-gray-500";

    private bool IsTestersNotLoaded { get; set; } = true;
    private Dictionary<string, Guid> testersDict = new();

    private List<string> OptionsTesters = new() { };
    List<TesterResponseDto> Testers = new();

    private string PlaceholderTesterText
    {
        get
        {
            return IsTestersNotLoaded
                ? "Cargando testers..."
                : "Selecciona un tester";
        }
    }

    private string? TesterSeleccionado;

    private string? DescripcionTesterSeleccionado;

    private bool IsTesterIdSelected = false;
    private bool IsNotTesterIdSelectedF = true;


    #endregion
    #endregion

    public async Task ShowAsync(Guid id)
    {
        Limpiar();  
        Id = id;
        await JS!.InvokeVoidAsync("modalHelpers.clickElement", showModalRef);
        IsLoadingData = true;
        await GetAllRequeriments();
        await GetAllTesters();
        StateHasChanged();
    }
    
    private void HandleDropdownStatusRequeriment(bool closed)
    {
        IsSelectClosedRequeriment = closed;
    }

    private void HandleDropdownStatusTester(bool closed)
    {
        IsSelectClosedTester = closed;
    }

    private async Task GetAllRequeriments()
    {
        IsLoadingData = true;
        IsRequerimentsNotLoaded = true;
        StateHasChanged();

        try
        {
            DataAResponse<RequerimentResponseDto> response = await RequerimentsService.GetAllRequeriments();
            Requeriments = response.Data;
            OptionsRequeriments = response.Data.Select(x => $"ReqID{x.ReqID}").ToList()!;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    
        IsLoadingData = false;
        IsRequerimentsNotLoaded = false;
        StateHasChanged();
        
    }

    private async Task GetAllTesters()
    {
        IsLoadingData = true;
        IsTestersNotLoaded = true;
        StateHasChanged();

        try
        {
            DataAResponse<TesterResponseDto> response = await TesterService.GetAllTesters();
            Testers = response.Data;
            testersDict = Testers.ToDictionary(t => t.Nombre, t => t.Id);
            OptionsTesters = response.Data.Select(t => t.Nombre).ToList()!;

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }


        IsLoadingData = false;
        IsTestersNotLoaded = false;
        StateHasChanged();
    }

    private void Limpiar()
    {
        createDto = new()
        {
            RequerimentId = default,
            TesterId = default
        };

        IsLoadingNew = false;

        isError = false;
        messageError = new MarkupString("");
        isSuccessReq = false;
        messageSuccessReq = new MarkupString("");

        RequerimientoSeleccionado = "";
        DescripcionRequerimientoSeleccionado = "";
        IsRequerimentIdSelected = false;
        IsNotRequerimentIdSelectedF = true;

        IsTesterIdSelected = false;
        IsNotTesterIdSelectedF = true;
    }

    private Task OnRequerimentSelectedChanged(string value)
    {
        if (value.Contains("ReqID"))
        {
            RequerimientoSeleccionado = value;
        }
        else
        {
            RequerimientoSeleccionado = $"ReqID{value}";
        }
        _ = SafeRunAsync(async () =>
        {
            DataResponse<RequerimentResponseDto> req = await RequerimentsService.GetRequerimentporReqId(value);
            createDto.RequerimentId = req.Data.Id;
            IsRequerimentIdSelected = true;
            IsNotRequerimentIdSelectedF = false;
            StateHasChanged();
        });
        
        StateHasChanged();
        return Task.CompletedTask;
    }

    private async Task SendCreateReject()
    {
        try
        {
            IsLoadingNew = true;
            isError = false;
            StateHasChanged();
            

            DataResponse<CreateRejectionResponseDto> response = await RejectionService!.CreateRejection(createDto);
            SavedEventArgs? args = new SavedEventArgs
            {
                Message = "",
                Success = false,
                StartDate = DateOnly.FromDateTime(DateTime.Now)
            };

            if (response?.Data.Id != Guid.Empty)
            {
                await JS!.InvokeVoidAsync("modalHelpers.clickElement", closeModalRef);
                IsLoadingNew = false;

                args.Success = true;
                args.Message = response!.Data.Message;
                args.IdResponse = response.Data.Id;
                args.Obs = response.Data.ReqId;
                args.ExtraData = response.Data.TesterId.ToString();

                Limpiar();
                await OnSaved.InvokeAsync(args);
            }
            else
            {
                isError = true;
                messageError = (MarkupString)(response?.Data.Message?.Replace("\n", "<br />") ?? "Error desconocido");
            }

            StateHasChanged();
        }
        catch (Exception e)
        {
            isError = true;
            messageError = new MarkupString(e.Message);
        }
        finally
        {
            IsLoadingNew = false;
            StateHasChanged();
        }
    }

    private Task OnTesterSelectedChanged(string value)
    {
        TesterSeleccionado = value;

        if (!string.IsNullOrEmpty(value) && testersDict.TryGetValue(value, out Guid testerId))
        {
            createDto.TesterId = testerId;
            IsTesterIdSelected = true;
            IsNotTesterIdSelectedF = false;
        }
        else
        {
            createDto.TesterId = default;
            IsTesterIdSelected = false;
            IsNotTesterIdSelectedF = true;
        }

        StateHasChanged();
        return Task.CompletedTask;
    }

}