using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.RejectionDetails;
using AppTiemposV3.SharedClases.DTOs.Testers;
using AppTiemposV3.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.Web.Utils.Helpers;
using static NanoidDotNet.Nanoid;
using static NanoidDotNet.Nanoid.Alphabets;
using static System.Globalization.CultureInfo;

namespace AppTiemposV3.Web.Pages.Rejections.ModalRejectionDetails;

public partial class NewRejectionDetailModal : ComponentBase
{
     #region "Variables" 
    public Guid Id { get; set; }
    [Inject] private IJSRuntime? JS { get; set; }
    
    [Inject] private NavigationManager? Router { get; set; }
    [Inject] private NotificationService Toltip { get; set; } = default!;
    [Inject] private IRejectionDetailContract<RejectionDetailResponseDto> RejectionDService { get; set; } = null!;
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

    private CreateRejectionDetailDto createDto = new()
    {
        TesterId = default
    };
    private bool IsLoadingNew = false;
    [Parameter] public EventCallback<SavedEventArgs> OnSaved { get; set; }
    private string ReqId {get; set;} = string.Empty;
    
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
    
    
    public async Task ShowAsync(Guid id, Guid rejectionId, string reqId, string tester)
    {
        await Limpiar();

        Id = id;
        ReqId = reqId;
        RejectionId = rejectionId;
        testerAnterior = tester;

        createDto = new() { RejectionId = rejectionId, TesterId = default };
        isError = false;
        messageError = new("");
        isSuccessReq = false;
        messageSuccessReq = new("");
        TesterSeleccionado = null;

        IsLoadingData = true;
        StateHasChanged(); 
        await JS!.InvokeVoidAsync("modalHelpers.clickElement", showModalRef);

        await SafeRunAsync(async () =>
        {
            await GetAllTesters();

            TesterSeleccionado = ObtenerTesterPorId(tester);

            if (!string.IsNullOrEmpty(TesterSeleccionado))
            {
                await OnTesterSelectedChanged(TesterSeleccionado);
            }

            IsLoadingData = false;
            StateHasChanged();
        });
    }

    private async Task Limpiar()
    {
        await JS!.InvokeVoidAsync("modalHelpers.clickElement", closeModalRef);

        createDto = new()
        {
            RejectionId = default,
            TesterId = default
        };

        IsLoadingNew = false;
        isError = false;
        messageError = new MarkupString("");
        isSuccessReq = false;
        messageSuccessReq = new MarkupString("");

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
    }


    private void HandleDropdownStatusTester(bool closed)
    {
        IsSelectClosedTester = closed;
    }

    private string RejectionDateString
    {
        get => createDto.RejectionDate.ToString("yyyy-MM-dd");
        set
        {
            if (DateOnly.TryParse(value, out DateOnly parsed))
            {
                createDto.RejectionDate = parsed;
                CheckIfPast();
            }
        }
    }

    private void CheckIfPast()
    {
        DateTime selectedDateTime = createDto.RejectionDate.ToDateTime(modalOpenedTime);
        bool wasPast = IsPastDateTime;
        
        IsPastDateTime = selectedDateTime < DateTime.Now;
        
        if (IsPastDateTime != wasPast)
            InvokeAsync(StateHasChanged);
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
            createDto.RejectionDate = DateOnly.FromDateTime(date.Value);
        }
    }
    
    private DateTime? SolutionDateForInput
    {
        get => createDto.RejectionDate.ToDateTime(modalOpenedTime);

        set
        {
            if (value.HasValue)
                createDto.RejectionDate = DateOnly.FromDateTime(value.Value);
        }
    }
    
    private void HandleDropdownState(bool closed)
    {
        IsSelectClosed = closed;
    }
    
    
    private async Task SendCreateRejectDetail()
    {
        try
        {
            createDto.RejectionId = RejectionId;
            PrintAsJson(createDto);
            IsLoadingNew = true;
            isError = false;
            StateHasChanged();
            

            GeneralResponse? response = await RejectionDService!.CreateRejectionDetail(createDto);
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
            IsLoadingNew = false;
            StateHasChanged();
        }
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