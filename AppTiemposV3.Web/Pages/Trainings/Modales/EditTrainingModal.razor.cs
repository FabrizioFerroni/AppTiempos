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
using System.Text.Json;

namespace AppTiemposV3.Web.Pages.Trainings.Modales;

public partial class EditTrainingModal : ComponentBase
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
    private bool IsLoadingInProgress = false;
    
    
    private string IdModalStr = Generate(LowercaseLettersAndDigits, 10);
    
    List<RequerimentResponseDto> Requeriments = new();
    
    private ElementReference showModalRef;
    private ElementReference closeModalRef;
    public DateOnly StartDate { get; set; }
    
    private bool isError = false;
    private MarkupString messageError = new("");

    private string modalStep = "selected-req";
    private List<string> OptionsRequeriments = new() {};
    private bool IsRequerimentsNotLoaded { get; set; } = true;
    private string? RequerimientoSeleccionado = "";
    private string? DescripcionRequerimientoSeleccionado = "";
    private bool IsRequerimentIdSelected = false;
    private bool IsNotRequerimentIdSelectedF = true;
    private bool IsPastDateEndTimeTime { get; set; } = false;
    private Guid IdModalRequeriment = Guid.NewGuid();
    private bool isSuccessReq;
    private MarkupString? messageSuccessReq = new("");
    private bool IsPastDateTime { get; set; } = false;
    
    private UpdateTrainingDto updateTrainingDto = new()
    {
        StartDate = DateOnly.FromDateTime(DateTime.Today),
        StartTime = TimeOnly.FromDateTime(DateTime.Now),
        Description = string.Empty,
        Notes = string.Empty,
        RequerimentId = Guid.Empty
    };
    private bool IsLoadingEdit = false;
    private Guid IdTraining = Guid.Empty;
    private TrainingResponseDto? OriginalTrainingData;
    
    private List<string> OptionsEstados = new() {"Pendiente", "En Progreso", "Completado"};
    
    private string? EstadoSeleccionado = "";
    [Parameter] public EventCallback<SavedEventArgs> OnSaved { get; set; }
    #endregion
    
    
    public async Task ShowAsync(Guid id, Guid idTraining)
    {
        await Limpiar();  
        Id = id;
        IdTraining = idTraining;

        await JS!.InvokeVoidAsync("modalHelpers.clickElement", showModalRef);
        StateHasChanged();

        IsLoadingData = true;
        StateHasChanged();
        IsLoadingData = false;
        await LoadDataAct(idTraining, true);
        StateHasChanged();

        await GetAllRequeriments();
        StateHasChanged();
    }
    
    private async Task LoadDataAct(Guid idTraining, bool saveOriginal)
    {
        try
        {
            if (IsLoadingInProgress)
                return;

            IsLoadingInProgress = true;
            IsLoadingData = true;
            StateHasChanged();

            DataResponse<TrainingResponseDto>? training =
                await TrainingService.GetTrainingPorId(idTraining);

            await FillDataCamps(training.Data);

            if (saveOriginal)
            {
                OriginalTrainingData = JsonSerializer.Deserialize<TrainingResponseDto>(
                    JsonSerializer.Serialize(training.Data)
                );
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            IsLoadingData = false;
            IsLoadingInProgress = false;
            StateHasChanged();
        }
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
    
    
    private void GoToReq()
    {
        SetModalStep("select-req");
        RequerimientoSeleccionado = string.Empty;
        DescripcionRequerimientoSeleccionado = string.Empty;
        updateTrainingDto.RequerimentId = Guid.Empty;
        IsNotRequerimentIdSelectedF = true;
    }
    
    private void SetModalStep(string step) => modalStep = step;
    
    private Task OnRequerimentSelectedChanged(string value)
    {
        // RequerimientoSeleccionado = value;
        if (value.Contains("ReqID"))
        {
            RequerimientoSeleccionado = value;
        }
        else
        {
            RequerimientoSeleccionado = $"ReqID{value}";
        }
        
        if (OriginalTrainingData is null || updateTrainingDto.RequerimentId != OriginalTrainingData!.Requeriment.Id)
        {
            _ = SafeRunAsync(async () =>
            {
                DataResponse<RequerimentResponseDto> req = await RequerimentsService.GetRequerimentporReqId(value);
                updateTrainingDto.RequerimentId = req.Data.Id;
                IsRequerimentIdSelected = true;
                DescripcionRequerimientoSeleccionado = req.Data.Titulo;
                IsNotRequerimentIdSelectedF = false;
                StateHasChanged();
            });
        }

        return Task.CompletedTask;
    }

    private void NextStep()
    {
        try
        {
            if (updateTrainingDto.RequerimentId != Guid.Empty)
            {
                IsNotRequerimentIdSelectedF = false;
                modalStep = "selected-req";
                updateTrainingDto.StartTime = TimeOnly.FromDateTime(DateTime.Now);
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
    
    private async Task FillDataCamps(TrainingResponseDto training)
    {
        updateTrainingDto.Id = training?.Id;
        updateTrainingDto.StartDate = training!.StartDate;
        if (TimeOnly.TryParse(training!.StartTime, out TimeOnly startTime))
        {
            updateTrainingDto.StartTime = startTime;
            CheckIfPast();
        }
        
        
        if (TimeOnly.TryParse(training!.EndTime, out TimeOnly endTime))
        {
            TimeOnly end = endTime;

            // Redondea al minuto más cercano (sin segundos ni milisegundos)
            if (end.Second >= 30)
            {
                end = end.AddMinutes(1);
            }

            end = new TimeOnly(end.Hour, end.Minute);

            // Asignalo al modelo
            updateTrainingDto.EndTime = end;
            CheckIfPastEndTime();
        }
        else
        {
            updateTrainingDto.EndTime = new TimeOnly(DateTime.Now.Hour, DateTime.Now.Minute);
        }
        
        updateTrainingDto.Description = training!.Description;
        updateTrainingDto.Capacitator = training!.Capacitator;
        updateTrainingDto.IsLoaded = training?.IsLoaded ?? false;
        updateTrainingDto.Notes = training!.Notes;
        _ = OnEstadoSelectedChangedBD(training!.Status);
        updateTrainingDto.Status = training!.Status;
        await OnRequerimentSelectedChanged(training!.Requeriment.ReqID);
        updateTrainingDto.RequerimentId = training?.Requeriment.Id;
            
        StateHasChanged();
    }
    
    private void CheckIfPastEndTime()
    {
        DateTime selectedDateTime = updateTrainingDto.StartDate.ToDateTime(updateTrainingDto.EndTime);
        bool wasPast = IsPastDateEndTimeTime;

        DateTime now = DateTime.Now;
        now = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

        IsPastDateEndTimeTime = selectedDateTime < now;

        if (IsPastDateEndTimeTime != wasPast)
            InvokeAsync(StateHasChanged);
    }
    
    private string OnEstadoSelectedChangedBD(string estado)
    {
        EstadoSeleccionado = GetStatusNameR(estado);
        return GetStatusNameR(estado);
        
    }
    
    private Task OnEstadoSelectedChanged(string estado)
    {
        EstadoSeleccionado = OnEstadoSelectedChangedBD(estado);
        updateTrainingDto.Status = GetStatusName(estado);
        return Task.CompletedTask;
        
    }
    
    private void HandleLoadedCheck(bool check)
    {
        updateTrainingDto.IsLoaded = check;
    }
    
    private string GetStatusName(string status)
    {
        switch (status) {
            case "Completado":
                return "completed";
            case "En Progreso":
                return "in-progress";
            case "Pendiente":
                return "pending";
            default:
                return status;
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
    
    private string StartDateString
    {
        get => updateTrainingDto.StartDate.ToString("yyyy-MM-dd"); // formato ISO compatible con <input type="date">
        set
        {
            if (DateOnly.TryParse(value, out DateOnly parsed))
            {
                updateTrainingDto.StartDate = parsed;
                CheckIfPast();
            }
        }
    }
    
    private string StartTimeString
    {
        get => updateTrainingDto.StartTime.ToString("HH:mm"); // formato 24h válido para <input type="time">
        set
        {
            if (TimeOnly.TryParse(value, out TimeOnly parsed))
            {
                updateTrainingDto.StartTime = parsed;
                CheckIfPast();
            }
        }
    }
    
    private string EndTimeString
    {
        get
        {
            string hora = "00:00:00";
            TimeOnly parsed = new TimeOnly(updateTrainingDto.EndTime.Hour, updateTrainingDto.EndTime.Minute);
            hora = parsed.ToString("HH:mm");
            return hora;
        }   
        set
        {
            if (TimeOnly.TryParse(value, out TimeOnly parsed))
            {
                parsed = new TimeOnly(parsed.Hour, parsed.Minute);
                updateTrainingDto.EndTime = parsed;
                CheckIfPastEndTime();
            }
        }
    }
    
    private string? IsLoadedString
    {
        get => updateTrainingDto.IsLoaded.ToString(); 
        set
        {
            if (bool.TryParse(value, out bool parsed))
            {
                updateTrainingDto.IsLoaded = parsed;
            }
        }
    }
    
    private void CheckIfPast()
    {
        DateTime selectedDateTime = updateTrainingDto.StartDate.ToDateTime(updateTrainingDto.StartTime);
        bool wasPast = IsPastDateTime;
        
        IsPastDateTime = selectedDateTime < DateTime.Now;
        
        if (IsPastDateTime != wasPast)
            InvokeAsync(StateHasChanged);
    }
    
    private async Task SendEditTraining()
    {
        try
        {
            PrintAsJson(updateTrainingDto);
            IsLoadingEdit = true;
            isError = false;
            StateHasChanged();

            GeneralResponse? response = await TrainingService!.UpdateTraining(IdTraining, updateTrainingDto);
            SavedEventArgs? args = new SavedEventArgs
            {
                Message = "",
                Success = false,
                StartDate = DateOnly.FromDateTime(DateTime.Now)
            };

            if (response?.Flag == true)
            {
                await JS!.InvokeVoidAsync("modalHelpers.clickElement", closeModalRef);
                IsLoadingEdit = false;
                args.Success = true;
                args.Message = response.Message;
                args.StartDate = StartDate;
                await Limpiar();
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
            IsLoadingEdit = false;
            StateHasChanged();
        }
    }
    
    /*private void Limpiar()
    {
        SetModalStep("selected-req");
        /*RequerimientoSeleccionado = "";
        DescripcionRequerimientoSeleccionado = "";*
        IsNotRequerimentIdSelectedF = false;
        updateTrainingDto = new()
        {
            RequerimentId = default
        };
        IsLoadingEdit = false;
    }*/
    
    private async Task Limpiar()
    {
        IsLoadingData = true;
        StateHasChanged();

        // Restaurar la copia original sin hacer request
        if (OriginalTrainingData != null)
        {
            TrainingResponseDto? resp =  JsonSerializer.Deserialize<TrainingResponseDto>(
                JsonSerializer.Serialize(OriginalTrainingData)
            );

            await FillDataCamps(resp!);
            RequerimientoSeleccionado = resp!.Requeriment.ReqIDDesc;
            DescripcionRequerimientoSeleccionado = resp!.Requeriment.Titulo;
        }

        await Task.Delay(100); // opcional, para un efecto visual de carga

        IsLoadingData = false;
        SetModalStep("selected-req");
        StateHasChanged();
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