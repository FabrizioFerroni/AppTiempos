using System.Globalization;
using System.Text.Json;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.RejectionDetails;
using AppTiemposV3.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using static NanoidDotNet.Nanoid;
using static NanoidDotNet.Nanoid.Alphabets;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.Web.Utils.Helpers;
using static System.Globalization.CultureInfo;

namespace AppTiemposV3.Web.Pages.Rejections.ModalRejectionDetails;

public partial class EditRejectionDetailModal : ComponentBase
{
    
    #region "Variables" 
    public Guid Id { get; set; }
    [Inject] private IJSRuntime? JS { get; set; }
    
    [Inject] private NavigationManager? Router { get; set; }
    [Inject] private NotificationService Toltip { get; set; } = default!;
    [Inject] private IRejectionDetailContract<RejectionDetailResponseDto> RejectionDService { get; set; } = null!;
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

    private UpdateRejectionDetailDto updateDto = new();
    private bool IsLoadingEdit = false;
    private RejectionDetailResponseDto? OriginalData;
    
    private string ReqID = string.Empty;
    
    private List<string> OptionsEstados = new() {"Pendiente", "En Progreso", "Completado"};
    
    private string? EstadoSeleccionado = "";
    [Parameter] public EventCallback<SavedEventArgs> OnSaved { get; set; }
    private TimeOnly modalOpenedTime;
    
    private int weekNumberSelected = 45;
    private DateTime currentDate = DateTime.Today;
    private bool IsSelectClosed = true;
    #endregion
    
    #region Inicializacion
    protected override void OnInitialized()
    {
        modalOpenedTime = TimeOnly.FromDateTime(DateTime.Now);
        weekNumberSelected = GetWeekNumber();
    }
    #endregion
    
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
            updateDto.SolutionDate = DateOnly.FromDateTime(date.Value);
        }
    }
    
    private DateTime? SolutionDateForInput
    {
        get => updateDto.SolutionDate.HasValue
            ? updateDto.SolutionDate.Value.ToDateTime(TimeOnly.MinValue)
            : null;

        set
        {
            if (value.HasValue)
                updateDto.SolutionDate = DateOnly.FromDateTime(value.Value);
            else
                updateDto.SolutionDate = null;
        }
    }
    
    private void HandleDropdownState(bool closed)
    {
        IsSelectClosed = closed;
    }
    
    public async Task ShowAsync(Guid id, string reqID, int rejectionNumber)
    {
        await Limpiar();  
        Id = id;
        ReqID = $"ReqID{reqID}";
        RejectionNumber = rejectionNumber;
        await JS!.InvokeVoidAsync("modalHelpers.clickElement", showModalRef);
        StateHasChanged();

        IsLoadingData = true;
        StateHasChanged();
        IsLoadingData = false;
        await LoadDataAct(id, true);
        StateHasChanged();

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

            DataResponse<RejectionDetailResponseDto>? rejectionDetail =
                await RejectionDService.GetRejectionDetailPorId(id);
            

            await FillDataCamps(rejectionDetail.Data);

            if (saveOriginal)
            {
                OriginalData = JsonSerializer.Deserialize<RejectionDetailResponseDto>(
                    JsonSerializer.Serialize(rejectionDetail.Data)
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
    
    private async Task FillDataCamps(RejectionDetailResponseDto rejectionDetail)
    {
        updateDto.Id = rejectionDetail.Id;
        updateDto.RejectionDate = rejectionDetail.RejectionDate;
        updateDto.RejectionReason = rejectionDetail.RejectionReason;
        updateDto.RejectionDetails = rejectionDetail.RejectionDetails;
        updateDto.SolutionDate = rejectionDetail!.SolutionDate;
        updateDto.SolutionDetails = rejectionDetail!.SolutionDetails;
        
        if (TimeOnly.TryParse(rejectionDetail!.EstimatedFixTime, out TimeOnly extimatedTime))
        {
            TimeOnly extimatedTimeR = extimatedTime;

            // Redondea al minuto más cercano (sin segundos ni milisegundos)
            if (extimatedTimeR.Second >= 30)
            {
                extimatedTimeR = extimatedTimeR.AddMinutes(1);
            }

            extimatedTimeR = new TimeOnly(extimatedTimeR.Hour, extimatedTimeR.Minute);

            // Asignalo al modelo
            updateDto.EstimatedFixTime = extimatedTimeR;
            CheckIfPastEndTime();

        }
        else
        {
            updateDto.EstimatedFixTime = new TimeOnly(DateTime.Now.Hour, DateTime.Now.Minute);
        }

        if (TimeOnly.TryParse(rejectionDetail!.ActualFixTime, out TimeOnly actualTime))
        {
            TimeOnly actualTimeR = actualTime;

            // Redondea al minuto más cercano (sin segundos ni milisegundos)
            if (actualTimeR.Second >= 30)
            {
                actualTimeR = actualTimeR.AddMinutes(1);
            }

            actualTimeR = new TimeOnly(actualTimeR.Hour, actualTimeR.Minute);

            // Asignalo al modelo
            updateDto.ActualFixTime = actualTimeR;
            CheckIfPastEndTime();
        }
        else
        {
            updateDto.ActualFixTime = new TimeOnly(DateTime.Now.Hour, DateTime.Now.Minute);
        }
        
        updateDto.RejectionId = rejectionDetail.RejectionId;
        _ = OnEstadoSelectedChangedBD(rejectionDetail!.Status);
        updateDto.Status = rejectionDetail!.Status;
            
        StateHasChanged();
    }
    
    private void CheckIfPast()
    {
        DateTime selectedDateTime = updateDto.RejectionDate.ToDateTime(modalOpenedTime);
        bool wasPast = IsPastDateTime;
        
        IsPastDateTime = selectedDateTime < DateTime.Now;
        
        if (IsPastDateTime != wasPast)
            InvokeAsync(StateHasChanged);
    }
    
    private void CheckIfPastEndTime()
    {
        DateTime selectedDateTime = updateDto.RejectionDate.ToDateTime(modalOpenedTime);
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
            RejectionDetailResponseDto? resp =  JsonSerializer.Deserialize<RejectionDetailResponseDto>(
                JsonSerializer.Serialize(OriginalData)
            );

            await FillDataCamps(resp!);
        }

        await Task.Delay(100); 

        IsLoadingData = false;
        StateHasChanged();
    }
    
    private async Task SendEditRejectDetail()
    {
        try
        {
            PrintAsJson(updateDto);
            IsLoadingEdit = true;
            isError = false;
            StateHasChanged();

            GeneralResponse? response = await RejectionDService!.UpdateRejectionDetail(Id, updateDto);
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
    
    
    private string RejectionDateString
    {
        get => updateDto.RejectionDate.ToString("yyyy-MM-dd"); // formato ISO compatible con <input type="date">
        set
        {
            if (DateOnly.TryParse(value, out DateOnly parsed))
            {
                updateDto.RejectionDate = parsed;
                CheckIfPast();
            }
        }
    }
    
    private string? SolutionDateString
    {
        get => updateDto.SolutionDate.ToString(); // formato ISO compatible con <input type="date">
        set
        {
            if (DateOnly.TryParse(value, out DateOnly parsed))
            {
                updateDto.SolutionDate = parsed;
                CheckIfPast();
            }
        }
    }
    
    private string? EstimatedFixTimeString
    {
        get => updateDto.EstimatedFixTime.ToString(); // formato 24h válido para <input type="time">
        set
        {
            if (TimeOnly.TryParse(value, out TimeOnly parsed))
            {
                updateDto.EstimatedFixTime = parsed;
                CheckIfPast();
            }
        }
    }
    
    private string? ActualFixTimeString
    {
        get => updateDto.ActualFixTime.ToString(); // formato 24h válido para <input type="time">
        set
        {
            if (TimeOnly.TryParse(value, out TimeOnly parsed))
            {
                updateDto.ActualFixTime = parsed;
                CheckIfPast();
            }
        }
    }
}