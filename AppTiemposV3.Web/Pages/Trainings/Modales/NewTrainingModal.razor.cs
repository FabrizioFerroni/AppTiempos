using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Activities;
using AppTiemposV3.SharedClases.DTOs.Requeriments;
using AppTiemposV3.SharedClases.DTOs.Trainings;
using AppTiemposV3.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using static NanoidDotNet.Nanoid;
using static NanoidDotNet.Nanoid.Alphabets;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.Web.Utils.Helpers;

namespace AppTiemposV3.Web.Pages.Trainings.Modales;

public partial class NewTrainingModal : ComponentBase
{
    #region "Variables" 
    public Guid Id { get; set; }
    [Inject] private IJSRuntime? JS { get; set; }
    
    [Inject] private NavigationManager? Router { get; set; }
    [Inject] private NotificationService Toltip { get; set; } = default!;
    [Inject] private ITrainingContract<TrainingResponseDto> TrainingService { get; set; } = null!;
    [Inject] private ColorService ColorService { get; set; } = null!;
    [Inject] private IRequerimentContract<RequerimentResponseDto> RequerimentsService { get; set; } = null!;
    [Inject] private IActivityContract<ActivityResponseDto>  ActivityService { get; set; } = null!;
    
    private RequerimentResponseDto requerimentShow = new RequerimentResponseDto();
    private Dictionary<string, (string Mensaje, bool EsExitoso)> mensajes = new();
    
    public bool IsLoadingData { get; set; } = true;
    private string IdModalStr = Generate(LowercaseLettersAndDigits, 10);
    
    List<RequerimentResponseDto> Requeriments = new();
    
    private ElementReference showModalRef;
    private ElementReference closeModalRef;
    public DateOnly StartDate { get; set; }
    
    private bool isError = false;
    private MarkupString messageError = new("");

    private string modalStep = "select-req";
    private List<string> OptionsRequeriments = new() {};
    private bool IsRequerimentsNotLoaded { get; set; } = true;
    private string? RequerimientoSeleccionado = "";
    private string? DescripcionRequerimientoSeleccionado = "";
    private bool IsRequerimentIdSelected = false;
    private bool IsNotRequerimentIdSelectedF = true;
    private Guid IdModalRequeriment = Guid.NewGuid();
    private bool isSuccessReq;
    private MarkupString? messageSuccessReq = new("");
    private bool IsPastDateTime { get; set; } = false;
    
    private CreateTrainingDto createTrainingDto = new()
    {
        StartDate = DateOnly.FromDateTime(DateTime.Today),
        StartTime = TimeOnly.FromDateTime(DateTime.Now),
        Description = string.Empty,
        Notes = string.Empty,
        RequerimentId = Guid.Empty
    };
    private bool IsLoadingNew = false;
    [Parameter] public EventCallback<SavedEventArgs> OnSaved { get; set; }
    #endregion
    
    
    public async Task ShowAsync(Guid id)
    {
        Limpiar();  
        Id = id;
        await JS!.InvokeVoidAsync("modalHelpers.clickElement", showModalRef);
        IsLoadingData = true;
        await GetAllRequeriments();
        StateHasChanged();
    }
    
    private async Task GetAllRequeriments()
    {
        IsLoadingData = true;
        IsRequerimentsNotLoaded = true;
        StateHasChanged(); // asegura render previo
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

    private void GoToReq() => Limpiar();
    
    private void SetModalStep(string step) => modalStep = step;
    
    private Task OnRequerimentSelectedChanged(string value)
    {
        RequerimientoSeleccionado = value;
        
        _ = SafeRunAsync(async () =>
        {
            DataResponse<RequerimentResponseDto> req = await RequerimentsService.GetRequerimentporReqId(value);
            createTrainingDto.RequerimentId = req.Data.Id;
            IsRequerimentIdSelected = true;
            DescripcionRequerimientoSeleccionado = req.Data.Titulo;
            IsNotRequerimentIdSelectedF = false;
            StateHasChanged();
        });

        return Task.CompletedTask;
    }

    private void NextStep()
    {
        try
        {
            if (createTrainingDto.RequerimentId != Guid.Empty)
            {
                IsNotRequerimentIdSelectedF = false;
                modalStep = "selected-req";
                createTrainingDto.StartTime = TimeOnly.FromDateTime(DateTime.Now);
                StateHasChanged();
            }
            else
            {
                IsNotRequerimentIdSelectedF = true;
                modalStep = "select-req";
                StateHasChanged();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    private string StartDateString
    {
        get => createTrainingDto.StartDate.ToString("yyyy-MM-dd"); // formato ISO compatible con <input type="date">
        set
        {
            if (DateOnly.TryParse(value, out DateOnly parsed))
            {
                createTrainingDto.StartDate = parsed;
                CheckIfPast();
            }
        }
    }
    
    private string StartTimeString
    {
        get => createTrainingDto.StartTime.ToString("HH:mm"); // formato 24h válido para <input type="time">
        set
        {
            if (TimeOnly.TryParse(value, out TimeOnly parsed))
            {
                createTrainingDto.StartTime = parsed;
                CheckIfPast();
            }
        }
    }
    
    private void CheckIfPast()
    {
        DateTime selectedDateTime = createTrainingDto.StartDate.ToDateTime(createTrainingDto.StartTime);
        bool wasPast = IsPastDateTime;
        
        IsPastDateTime = selectedDateTime < DateTime.Now;
        
        if (IsPastDateTime != wasPast)
            InvokeAsync(StateHasChanged);
    }

    private async Task SendNewTraining()
    {
        try
        {
            PrintAsJson(createTrainingDto);
            IsLoadingNew = true;
            isError = false;
            StateHasChanged();

            GeneralResponse? response = await TrainingService!.CreateTraining(createTrainingDto);
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
                Limpiar();
                await OnSaved.InvokeAsync(args);
            }
            else
            {
                isError = true;
                messageError = (MarkupString)(response?.Message?.Replace("\n", "<br />") ?? "Error desconocido");
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

    private void Limpiar()
    {
        SetModalStep("select-req");
        RequerimientoSeleccionado = "";
        DescripcionRequerimientoSeleccionado = "";
        IsNotRequerimentIdSelectedF = true;
        createTrainingDto = new()
        {
            RequerimentId = default
        };
        IsLoadingNew = false;
    }
    
    
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
    
}