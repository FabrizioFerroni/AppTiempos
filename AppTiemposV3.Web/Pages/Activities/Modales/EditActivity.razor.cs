using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Activities;
using AppTiemposV3.SharedClases.DTOs.Requeriments;
using AppTiemposV3.SharedClases.Enums;
using AppTiemposV3.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
// using Newtonsoft.Json;
using static NanoidDotNet.Nanoid;
using static NanoidDotNet.Nanoid.Alphabets;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using System.Text.Json;

namespace AppTiemposV3.Web.Pages.Activities.Modales;

public partial class EditActivity : ComponentBase
{
    #region Parameters-Variables
    public Guid Id { get; set; }
    public Guid IdActivity { get; set; }
    [Inject] ActivityStateService ActivityState { get; set; } = null!;
    [Inject] private IJSRuntime? JS { get; set; }
    [Inject] private ColorService ColorService { get; set; } = null!;
    [Inject] private IRequerimentContract<RequerimentResponseDto> RequerimentsService { get; set; } = null!;
    [Inject] private IActivityContract<ActivityResponseDto>  ActivityService { get; set; } = null!;
    [Parameter] public EventCallback<SavedEventArgs> OnSaved { get; set; }
    
    private string IdModalStr = Generate(LowercaseLettersAndDigits, 10);
    
    private bool isError = false;
    private bool IsPastDateTime { get; set; } = false;
    private bool IsPastDateEndTimeTime { get; set; } = false;
    private MarkupString messageError = new("");

    private UpdateActivityDto activityDto = new();

    private ActivityResponseDto? OriginalActivityData;
    
    private ValidationResultDto validationResult = new()
    {
        IsValid = true,
    };
    
    private bool IsLoadingEdit = false;
    private bool IsLoadingData { get; set; } = true;
    private bool IsLoadingInProgress = false;
    
    private ElementReference showModalRef;
    private ElementReference closeModalRef;
    
    //Requerimientos
    private bool isSuccessReq;
    private MarkupString? messageSuccessReq = new("");
    private string? RequerimientoSeleccionado = "";
    private string? EstadoSeleccionado = "";
    private List<string> OptionsRequeriments = new() {};
    private List<string> OptionsEstados = new() {"Pendiente", "En Progreso", "Completado"};
    private bool IsRequerimentIdSelected = false;
    private Guid IdModalRequeriment = Guid.NewGuid();
    private bool IsRequerimentsLoaded = true;
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
    
    public async Task ShowAsync(Guid id, Guid idActivity)
    {
        Id = id;
        IdActivity = idActivity;
        //CheckIfPastEndTime();

        await JS!.InvokeVoidAsync("modalHelpers.clickElement", showModalRef);
        StateHasChanged();

        // Mostrar pantalla de carga
        IsLoadingData = true;
        StateHasChanged();
        IsLoadingData = false;
        await LoadDataAct(idActivity, true);
        StateHasChanged();

        await GetAllRequeriments();
        StateHasChanged();

    }
    
    private async Task LoadDataAct(Guid idActivity, bool saveOriginal)
    {
        try
        {
            if (IsLoadingInProgress)
                return;

            IsLoadingInProgress = true;
            IsLoadingData = true;
            StateHasChanged();

            DataResponse<ActivityResponseDto>? activity =
                await ActivityService.GetActivityById(idActivity);

            await FillDataCamps(activity.Data);

            if (saveOriginal)
            {
                OriginalActivityData = JsonSerializer.Deserialize<ActivityResponseDto>(
                    JsonSerializer.Serialize(activity.Data)
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
    #endregion
    
    #region Funciones

    private async Task FillDataCamps(ActivityResponseDto activity)
    {
        activityDto.Id = activity?.Id;
        activityDto.StartDate = activity!.StartDate;
        if (TimeOnly.TryParse(activity!.StartTime, out TimeOnly startTime))
        {
            activityDto.StartTime = startTime;
            CheckIfPast();
        }
        if (TimeOnly.TryParse(activity!.EndTime, out TimeOnly endTime))
        {
            Console.WriteLine(endTime);
            TimeOnly end = endTime;

            // Redondea al minuto más cercano (sin segundos ni milisegundos)
            if (end.Second >= 30)
            {
                end = end.AddMinutes(1);
            }

            end = new TimeOnly(end.Hour, end.Minute);

            // Asignalo al modelo
            activityDto.EndTime = end;
            CheckIfPastEndTime();
        }
        else
        {
            activityDto.EndTime = new TimeOnly(DateTime.Now.Hour, DateTime.Now.Minute);
        }
        
        activityDto.Description = activity!.Description;
        await OnEtapaSelectedChanged(activity!.Etapa);
        activityDto.IsLoaded = activity?.IsLoaded ?? false;
        activityDto.Comment = activity!.Comment;
        _ = OnEstadoSelectedChangedBD(activity!.StatusMessage);
        activityDto.StatusMessage = activity!.StatusMessage;
        await OnRequerimentSelectedChanged(activity!.Requeriment.ReqID);
        activityDto.RequerimentId = activity?.Requeriment.Id;
            
        StateHasChanged();
    }
    
    private Task OnEstadoSelectedChanged(string estado)
    {
        EstadoSeleccionado = OnEstadoSelectedChangedBD(estado);
        activityDto.StatusMessage = GetStatusName(estado);
        return Task.CompletedTask;
        
    }
    
    private string OnEstadoSelectedChangedBD(string estado)
    {
        EstadoSeleccionado = GetStatusNameR(estado);
        return GetStatusNameR(estado);
        
    }
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
    
    private void HandleLoadedCheck(bool check)
    {
        activityDto.IsLoaded = check;
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
        IsRequerimentsLoaded = true;
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
        finally
        {
            IsRequerimentsLoaded = false;
        }
    }
    private string StartDateString
    {
        get => activityDto.StartDate.ToString("yyyy-MM-dd"); // formato ISO compatible con <input type="date">
        set
        {
            if (DateOnly.TryParse(value, out DateOnly parsed))
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
            if (TimeOnly.TryParse(value, out TimeOnly parsed))
            {
                activityDto.StartTime = parsed;
                CheckIfPast();
            }
        }
    }
    
    
    private string EndTimeString
    {
        get
        {
            string hora = "00:00:00";
            TimeOnly parsed = new TimeOnly(activityDto.EndTime.Hour, activityDto.EndTime.Minute);
            hora = parsed.ToString("HH:mm");
            return hora;
        }   
        set
        {
            if (TimeOnly.TryParse(value, out TimeOnly parsed))
            {
                //activityDto.EndTime = parsed;
                parsed = new TimeOnly(parsed.Hour, parsed.Minute);
                activityDto.EndTime = parsed;
                CheckIfPastEndTime();
            }
        }
    }
    
    private string? IsLoadedString
    {
        get => activityDto.IsLoaded.ToString(); 
        set
        {
            if (bool.TryParse(value, out bool parsed))
            {
                activityDto.IsLoaded = parsed;
            }
        }
    }
    /*private void CheckIfPast()
    {
        DateTime selectedDateTime = activityDto.StartDate.ToDateTime(activityDto.StartTime);
        bool wasPast = IsPastDateTime;
        
        IsPastDateTime = selectedDateTime < DateTime.Now;
        
        if (IsPastDateTime != wasPast)
            InvokeAsync(StateHasChanged);
    }
    
    private void CheckIfPastEndTime()
    {
        DateTime selectedDateTime = activityDto.StartDate.ToDateTime(activityDto.EndTime);
        bool wasPast = IsPastDateEndTimeTime;
        
        IsPastDateEndTimeTime = selectedDateTime < DateTime.Now;
        
        if (IsPastDateEndTimeTime != wasPast)
            InvokeAsync(StateHasChanged);
    }
    */
    
    private void CheckIfPast()
    {
        DateTime selectedDateTime = activityDto.StartDate.ToDateTime(activityDto.StartTime);
        bool wasPast = IsPastDateTime;

        DateTime now = DateTime.Now;
        now = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

        IsPastDateTime = selectedDateTime < now;

        if (IsPastDateTime != wasPast)
            InvokeAsync(StateHasChanged);
    }

    private void CheckIfPastEndTime()
    {
        DateTime selectedDateTime = activityDto.StartDate.ToDateTime(activityDto.EndTime);
        bool wasPast = IsPastDateEndTimeTime;

        DateTime now = DateTime.Now;
        now = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

        IsPastDateEndTimeTime = selectedDateTime < now;

        if (IsPastDateEndTimeTime != wasPast)
            InvokeAsync(StateHasChanged);
    }
    private async Task SendEditActivity()
    {
        try
        {
            IsLoadingEdit = true;
            isError = false;
            StateHasChanged();
            GeneralResponse? response = await ActivityService!.UpdateActivity(IdActivity, activityDto);
            
            if (response?.Flag == true)
            {
                ActivityState.NotifyActivityUpdated();
                _ = SafeRunAsync(async () => await OnResetEditAct(IdActivity));
                IsLoadingEdit = false;
                await JS!.InvokeVoidAsync("modalHelpers.clickElement", closeModalRef);
                SavedEventArgs? args = new SavedEventArgs
                {
                    Message = response.Message,
                    Success = response.Flag,
                    StartDate = activityDto.StartDate
                };
                await OnSaved.InvokeAsync(args);

            }
            else
            {
                isError = true;
                messageError = (MarkupString)(response?.Message?.Replace("\n", "<br />") ?? "Error desconocido");
            }
        }
        catch (Exception ex)
        {
            isError = true;
            messageError = new MarkupString(ex.Message);
        }
        finally
        {
            IsLoadingEdit = false;
            StateHasChanged();
        }
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

        if (OriginalActivityData is null || activityDto.RequerimentId != OriginalActivityData!.Requeriment.Id)
        {
            _ = SafeRunAsync(async () =>
            {
                DataResponse<Guid> requerimentId = await ActivityService.GetRequerimentActivity(value);
                activityDto.RequerimentId = requerimentId.Data;
                IsRequerimentIdSelected = true;
                StateHasChanged();
            });
        }

        return Task.CompletedTask;
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
    
    /*private async Task OnResetEditAct(Guid id)
    {
        IsLoadingData = false;
        StateHasChanged();
        await LoadDataAct(id, false);
    }*/

    private async Task OnResetEditAct(Guid idActivity)
    {
        IsLoadingData = true;
        StateHasChanged();

        // Restaurar la copia original sin hacer request
        if (OriginalActivityData != null)
        {
            ActivityResponseDto? resp =  JsonSerializer.Deserialize<ActivityResponseDto>(
                JsonSerializer.Serialize(OriginalActivityData)
            );

            await FillDataCamps(resp!);
        }

        await Task.Delay(100); // opcional, para un efecto visual de carga

        IsLoadingData = false;
        StateHasChanged();
    }
}