using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Activities;
using AppTiemposV3.SharedClases.DTOs.Requeriments;
using AppTiemposV3.SharedClases.Enums;
using AppTiemposV3.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using static NanoidDotNet.Nanoid;
using static NanoidDotNet.Nanoid.Alphabets;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.Web.Components.UI;

public partial class NewActivitySidebar : ComponentBase
{
    #region Parameters-Variables
    public Guid Id { get; set; }
    [Inject] private IJSRuntime? JS { get; set; }
    [Inject] private ColorService ColorService { get; set; } = null!;
    [Inject] private IRequerimentContract<RequerimentResponseDto> RequerimentsService { get; set; } = null!;
    [Inject] private IActivityContract<ActivityResponseDto>  ActivityService { get; set; } = null!;
    [Inject] private NotificationService Toltip { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = null!;
    [Parameter] public EventCallback<SavedEventArgs> OnSaved { get; set; }
    //[Parameter] public EventCallback<DateOnly> OnSaved { get; set; }
    
    
    public DateOnly StartDate { get; set; }
    
    private string CurrentUrl => Nav.Uri.Replace(Nav.BaseUri, "/");
    
    private string IdModalStr = Generate(LowercaseLettersAndDigits, 10);
    
    private bool isError = false;
    private bool IsPastDateTime { get; set; } = false;
    private MarkupString messageError = new("");
    private CreateActivityDto activityDto = new()
    {
        StartDate = DateOnly.FromDateTime(DateTime.Today),
        StartTime = TimeOnly.FromDateTime(DateTime.Now),
        Description = string.Empty,
        Etapa = Etapas.Alta,
        RequerimentId = Guid.Empty
    };
    
    private ValidationResultDto validationResult = new()
    {
        IsValid = true,
    };
    
    private bool IsLoadingNew = false;
    
    private ElementReference showModalRef;
    private ElementReference closeModalRef;
    
    //Requerimientos
    private bool isSuccessReq;
    private MarkupString? messageSuccessReq = new("");
    private string? RequerimientoSeleccionado = "";
    private List<string> OptionsRequeriments = new() {};
    private bool IsRequerimentIdSelected = false;
    private Guid IdModalRequeriment = Guid.NewGuid();
    private bool IsRequerimentsNotLoaded { get; set; } = true;
    private List<Etapas> OptionsEtapas = Enum.GetValues(typeof(Etapas))
        .Cast<Etapas>()
        .Where(e => e != Etapas.None)
        .ToList();
    private Etapas? EtapaSeleccionadaNullable = Etapas.Alta;
    
    private Etapas EtapaSeleccionada
    {
        get => EtapaSeleccionadaNullable ?? default; // default es el primer valor del enum
        set => EtapaSeleccionadaNullable = value;
    }
    #endregion
    #region Inicializacion
    protected async override Task OnAfterRenderAsync(bool firstRender)
    {
        string currentUrl = CurrentUrl;
        if (firstRender && currentUrl.StartsWith("/app/", StringComparison.OrdinalIgnoreCase))
        {
            await GetAllRequeriments();
            StateHasChanged();
        }
    }

    public async Task ShowAsync(Guid id, DateOnly startDate)
    {
        Id = id;
        StartDate = startDate;
        await JS!.InvokeVoidAsync("modalHelpers.clickElement", showModalRef);
        StateHasChanged();
    }
    #endregion
    
    #region Funciones
    private Task OnEtapaSelectedChanged(Etapas value)
    {
        EtapaSeleccionada = value;
        activityDto!.Etapa = value;
        return Task.CompletedTask;
    }
    private async Task HandleRequerimentSaved()
    {
        await GetAllRequeriments();
    }
    
    private void HandleRequerimentSelected(string reqId)
    {
        OnRequerimentSelectedChanged(reqId);
    }
    
    private async Task HandleRequerimentSuccess(MarkupString msg)
    {
        await ShowSuccess(msg);
    }
    
    private async Task ShowSuccess(MarkupString message)
    {
        messageSuccessReq = message;
        StateHasChanged();

        await Task.Delay(5000);

        messageSuccessReq = null;
        StateHasChanged();
        
        isSuccessReq = false;
    }
    
    public async Task GetAllRequeriments()
    {
        IsRequerimentsNotLoaded = true;
         StateHasChanged(); // asegura render previo
        try
        {
            DataAResponse<RequerimentResponseDto> requeriments = await RequerimentsService.GetAllRequeriments();
            OptionsRequeriments = requeriments.Data.Select(x => $"ReqID{x.ReqID}").ToList()!;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
        IsRequerimentsNotLoaded = false;
        StateHasChanged();
    }
    private string StartDateString
    {
        get => activityDto.StartDate.ToString("yyyy-MM-dd"); // formato ISO compatible con <input type="date">
        set
        {
            if (DateOnly.TryParse(value, out var parsed))
            {
                activityDto.StartDate = parsed;
                CheckIfPast();
            }
        }
    }
    
    private string StartTimeString
    {
        get => activityDto.StartTime.ToString("HH:mm"); // formato 24h válido para <input type="time">
        set
        {
            if (TimeOnly.TryParse(value, out var parsed))
            {
                activityDto.StartTime = parsed;
                CheckIfPast();
            }
        }
    }
    
    private void CheckIfPast()
    {
        DateTime selectedDateTime = activityDto.StartDate.ToDateTime(activityDto.StartTime);
        bool wasPast = IsPastDateTime;
        
        IsPastDateTime = selectedDateTime < DateTime.Now;
        
        if (IsPastDateTime != wasPast)
            InvokeAsync(StateHasChanged);
    }
    
    private async Task SendNewActivity()
    {
        Console.WriteLine($"Actividad a crear: {JsonConvert.SerializeObject(activityDto)}");
        try
        {
            IsLoadingNew = true;
            isError = false;
            StateHasChanged();
            
            GeneralResponse? response = await ActivityService!.CreateActivity(activityDto);
            SavedEventArgs? args = new SavedEventArgs
            {
                Message = "",
                Success = false,
                StartDate = DateOnly.FromDateTime(DateTime.Now)
            };
            
            if (response?.Flag == true)
            {
                await JS!.InvokeVoidAsync("modalHelpers.clickElement", closeModalRef);
                IsLoadingNew = false;
                args.Success = true;
                args.Message = response.Message;
                args.StartDate = StartDate;
                OnResetNewAct();
                await OnSaved.InvokeAsync(args);
            }
            else
            {
                isError = true;
                messageError = (MarkupString)(response?.Message?.Replace("\n", "<br />") ?? "Error desconocido");
            }
            
            StateHasChanged();
        }
        catch (Exception ex)
        {
            isError = true;
            messageError = new MarkupString(ex.Message);
        }
        finally
        {
            IsLoadingNew = false;
            StateHasChanged();
        }
    }
    
    private Task OnRequerimentSelectedChanged(string value)
    {
        RequerimientoSeleccionado = value;
        
        _ = SafeRunAsync(async () =>
        {
            DataResponse<Guid> requerimentId = await ActivityService.GetRequerimentActivity(value);
            activityDto.RequerimentId = requerimentId.Data;
            IsRequerimentIdSelected = true;
            StateHasChanged();
        });

        return Task.CompletedTask;
    }
    #endregion
    
    
    #region Scripts
    private async Task SafeRunAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error en fire & forget: {ex}");
        }
    }
    #endregion
    
    private void OnResetNewAct()
    {
        RequerimientoSeleccionado = "";
        activityDto = new()
        {
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            StartTime = TimeOnly.FromDateTime(DateTime.Now),
            Description = string.Empty,
            Etapa = Etapas.Alta,
            RequerimentId = Guid.Empty
        };
    }
}