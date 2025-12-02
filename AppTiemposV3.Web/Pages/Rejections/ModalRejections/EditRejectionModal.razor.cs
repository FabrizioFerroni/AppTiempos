using System.Globalization;
using System.Text.Json;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Rejections;
using AppTiemposV3.SharedClases.DTOs.Requeriments;
using AppTiemposV3.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using static NanoidDotNet.Nanoid;
using static NanoidDotNet.Nanoid.Alphabets;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.Web.Utils.Helpers;
using static System.Globalization.CultureInfo;

namespace AppTiemposV3.Web.Pages.Rejections.ModalRejections;

public partial class EditRejectionModal : ComponentBase
{
        #region "Variables" 
    public Guid Id { get; set; }
    [Inject] private IJSRuntime? JS { get; set; }
    
    [Inject] private NavigationManager? Router { get; set; }
    [Inject] private NotificationService Toltip { get; set; } = default!;
    [Inject] private IRejectionContract<RejectionResponseDto> RejectionService { get; set; } = null!;
    [Inject] private ColorService ColorService { get; set; } = null!;
    
    private Dictionary<string, (string Mensaje, bool EsExitoso)> mensajes = new();
    
    public bool IsLoadingData { get; set; } = true;
    private bool IsLoadingInProgress = false;
    
    
    private string IdModalStr = Generate(LowercaseLettersAndDigits, 10);
    private int RejectionNumber = 0;
    
    
    private ElementReference showModalRef;
    private ElementReference closeModalRef;
    
    private bool isError = false;
    private MarkupString messageError = new("");

    private bool IsPastDateEndTimeTime { get; set; } = false;
    private bool isSuccessReq;
    private MarkupString? messageSuccessReq = new("");
    private bool IsPastDateTime { get; set; } = false;

    private UpdateRejectionDto updateDto = new();
    private bool IsLoadingEdit = false;
    private RejectionResponseDto? OriginalData;
    
    private string ReqID = string.Empty;
    
    private List<string> OptionsEstados = new() {"Pendiente", "En Progreso", "Completado"};
    
    private string? EstadoSeleccionado = "";
    [Parameter] public EventCallback<SavedEventArgs> OnSaved { get; set; }
    private TimeOnly modalOpenedTime;
    
    private int weekNumberSelected = 45;
    private DateTime currentDate = DateTime.Today;
    private bool IsSelectClosed = true;
    private bool IsSelectClosedStatus = true;
    
    #region Requeriments
    private List<string> OptionsRequeriments = new() {};
    private bool IsRequerimentsNotLoaded { get; set; } = true;
    private string? RequerimientoSeleccionado = "";
    private string? DescripcionRequerimientoSeleccionado = "";
    private bool IsRequerimentIdSelected = false;
    private bool IsNotRequerimentIdSelectedF = true;
    [Inject] private IRequerimentContract<RequerimentResponseDto> RequerimentsService { get; set; } = null!;
    List<RequerimentResponseDto> Requeriments = new();
    #endregion
    #endregion
    
    #region Inicializacion
    protected override void OnInitialized()
    {
        modalOpenedTime = TimeOnly.FromDateTime(DateTime.Now);
        weekNumberSelected = GetWeekNumber();
    }
    #endregion
    
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
        
        if (OriginalData is null || updateDto.RequerimentId != OriginalData!.Requeriment.Id)
        {
            _ = SafeRunAsync(async () =>
            {
                DataResponse<RequerimentResponseDto> req = await RequerimentsService.GetRequerimentporReqId(value);
                updateDto.RequerimentId = req.Data.Id;
                IsRequerimentIdSelected = true;
                DescripcionRequerimientoSeleccionado = req.Data.Titulo;
                IsNotRequerimentIdSelectedF = false;
                StateHasChanged();
            });
        }

        return Task.CompletedTask;
    }
    
    int GetWeekNumber()
    {
        CultureInfo ci = CurrentCulture;

        return ci.Calendar.GetWeekOfYear(
            currentDate,
            CalendarWeekRule.FirstFourDayWeek,  
            DayOfWeek.Monday                    
        );
    }
    
    
    private void DateChangedFrom(DateTime? date)
    {
        if (date != null)
        {
            PrintAsJson(date);
            updateDto.ResolvedDate = date;
        }
    }
    
    private DateTime? ResolvedDateForInput
    {
        get => updateDto.ResolvedDate;

        set
        {
            if (value.HasValue)
                updateDto.ResolvedDate = value.Value;
            else
                updateDto.ResolvedDate = null;
        }
    }
    
    private void HandleDropdownState(bool closed)
    {
        IsSelectClosed = closed;
    }

    private void HandleDropdownStatusState(bool closed)
    {
        IsSelectClosedStatus = closed;
    }
    
    
    private void HandleResolveCheck(bool check)
    {
        updateDto.IsResolve = check;
        StateHasChanged();
    }
    
    public async Task ShowAsync(Guid id, string reqId)
    {
        await Limpiar();  
        Id = id;
        ReqID = $"ReqID{reqId}";
        await JS!.InvokeVoidAsync("modalHelpers.clickElement", showModalRef);
        StateHasChanged();

        IsLoadingData = true;
        StateHasChanged();
        IsLoadingData = false;
        await LoadDataAct(id, true);
        StateHasChanged();

        await GetAllRequeriments();
        StateHasChanged();
    }
    
    private async Task LoadDataAct(Guid id, bool saveOriginal)
    {
        try
        {
            if (IsLoadingInProgress)
                return;

            IsLoadingInProgress = true;
            IsLoadingData = true;
            StateHasChanged();

            DataResponse<RejectionResponseDto>? rejection =
                await RejectionService.GetRejectionPorId(id);
            

            await FillDataCamps(rejection.Data);

            if (saveOriginal)
            {
                OriginalData = JsonSerializer.Deserialize<RejectionResponseDto>(
                    JsonSerializer.Serialize(rejection.Data)
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
    
    private async Task FillDataCamps(RejectionResponseDto rejection)
    {
        updateDto.Id = rejection.Id;
        updateDto.ResolvedDate = rejection.ResolvedDate;
        updateDto.IsResolve = rejection.IsResolve;
        
        updateDto.RequerimentId = rejection.Requeriment.Id;
        _ = OnEstadoSelectedChangedBD(rejection!.Status);
        updateDto.Status = rejection!.Status;
        await OnRequerimentSelectedChanged(rejection!.Requeriment.ReqID);
        updateDto.RequerimentId = rejection!.Requeriment.Id;
        updateDto.TotalRejections = rejection!.TotalRejections;
        
        StateHasChanged();
    }
    
    private void CheckIfPast()
    {
        DateTime? selectedDateTime = updateDto.ResolvedDate;
        bool wasPast = IsPastDateTime;
        
        IsPastDateTime = selectedDateTime < DateTime.Now;
        
        if (IsPastDateTime != wasPast)
            InvokeAsync(StateHasChanged);
    }
    
    private void CheckIfPastEndTime()
    {
        DateTime? selectedDateTime = updateDto.ResolvedDate;
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
        updateDto.Status = GetStatusName(estado);
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
    
    
    private async Task Limpiar()
    {
        IsLoadingData = true;
        StateHasChanged();

        // Restaurar la copia original sin hacer request
        if (OriginalData != null)
        {
            RejectionResponseDto? resp =  JsonSerializer.Deserialize<RejectionResponseDto>(
                JsonSerializer.Serialize(OriginalData)
            );

            await FillDataCamps(resp!);
        }

        await Task.Delay(100); 

        IsLoadingData = false;
        StateHasChanged();
    }
    
    private async Task SendEditReject()
    {
        try
        {
            PrintAsJson(updateDto);
            IsLoadingEdit = true;
            isError = false;
            StateHasChanged();

            GeneralResponse? response = await RejectionService!.UpdateRejection(Id, updateDto);
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