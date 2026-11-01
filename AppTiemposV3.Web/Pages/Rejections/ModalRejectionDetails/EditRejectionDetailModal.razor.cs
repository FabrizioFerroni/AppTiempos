using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.RejectionDetails;
using AppTiemposV3.SharedClases.DTOs.Testers;
using AppTiemposV3.SharedClases.Enums;
using AppTiemposV3.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Text.Json;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.Web.Utils.Helpers;
using static NanoidDotNet.Nanoid;
using static NanoidDotNet.Nanoid.Alphabets;
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
    [Inject] private ITesterContract<TesterResponseDto> TesterService { get; set; } = null!;
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

    private bool IsSelectClosedTester = true;
    private bool IsTestersNotLoaded { get; set; } = true;
    private Dictionary<Guid, string> testersByIdDict = new();
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
    private string? testerAnterior = string.Empty;
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

    private void HandleDropdownStatusTester(bool closed)
    {
        IsSelectClosedTester = closed;
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
      


        await GetAllTesters();
        
        await LoadDataAct(id, true);
        
        StateHasChanged();
    }

    private async Task GetAllTesters()
    {
        IsTestersNotLoaded = true;
        StateHasChanged();

        try
        {
            DataAResponse<TesterResponseDto> response = await TesterService.GetAllTesters();
            Testers = response.Data;
            testersDict = Testers.ToDictionary(t => t.Nombre, t => t.Id);
            testersByIdDict = Testers.ToDictionary(t => t.Id, t => t.Nombre);
            OptionsTesters = response.Data.Select(t => t.Nombre).ToList()!;

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        IsTestersNotLoaded = false;
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
        
        updateDto.RejectionId = rejectionDetail.RejectionId;
        _ = OnEstadoSelectedChangedBD(rejectionDetail!.Status);
        updateDto.Status = rejectionDetail!.Status;
        updateDto.BadRejected = rejectionDetail.BadRejected;
        updateDto.ReasonBadRequest = rejectionDetail.ReasonBadRequest;

        await SafeRunAsync(async () =>
        {

            TesterSeleccionado = ObtenerTesterPorId(rejectionDetail.Tester?.Id.ToString()!);

            if (!string.IsNullOrEmpty(TesterSeleccionado))
            {
                await OnTesterSelectedChanged(TesterSeleccionado);
            }

            IsLoadingData = false;
            StateHasChanged();
        });

        testerAnterior = rejectionDetail.Tester?.Id.ToString();

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

    private void HandleBadRejectedCheck(bool check)
    {
        updateDto.BadRejected = check;
        StateHasChanged();
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

        await Task.Delay(250); 

        IsLoadingData = false;

        if (!string.IsNullOrWhiteSpace(testerAnterior) && Guid.TryParse(testerAnterior, out _))
        {
            TesterSeleccionado = ObtenerTesterPorId(testerAnterior);
            if (!string.IsNullOrEmpty(TesterSeleccionado))
            {
                await OnTesterSelectedChanged(TesterSeleccionado);
            }
        }
        else
        {
            TesterSeleccionado = null;
        }
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
        get => updateDto.RejectionDate.ToString("yyyy-MM-dd");
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
        get => updateDto.SolutionDate.ToString();
        set
        {
            if (DateOnly.TryParse(value, out DateOnly parsed))
            {
                updateDto.SolutionDate = parsed;
                CheckIfPast();
            }
        }
    }

    private Task OnTesterSelectedChanged(string value)
    {
        TesterSeleccionado = value;

        if (!string.IsNullOrEmpty(value) && testersDict.TryGetValue(value, out Guid testerId))
        {
            updateDto.TesterId = testerId;
            IsTesterIdSelected = true;
            IsNotTesterIdSelectedF = false;
        }
        else
        {
            updateDto.TesterId = default;
            IsTesterIdSelected = false;
            IsNotTesterIdSelectedF = true;
        }

        return Task.CompletedTask;
    }

    private string ObtenerTesterPorId(string id)
    {
        if (Guid.TryParse(id, out Guid testerGuid))
        {
            if (testersByIdDict.TryGetValue(testerGuid, out string? nombre))
            {
                return nombre;
            }
        }
        else if (!string.IsNullOrEmpty(id) && testersDict.ContainsKey(id))
        {
            return id;
        }

        return string.Empty;
    }
}