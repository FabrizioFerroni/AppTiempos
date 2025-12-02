using System.Globalization;
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

public partial class NewRejectionDetailModal : ComponentBase
{
     #region "Variables" 
    public Guid Id { get; set; }
    [Inject] private IJSRuntime? JS { get; set; }
    
    [Inject] private NavigationManager? Router { get; set; }
    [Inject] private NotificationService Toltip { get; set; } = default!;
    [Inject] private IRejectionDetailContract<RejectionDetailResponseDto> RejectionDService { get; set; } = null!;
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
    
    private CreateRejectionDetailDto createDto = new();
    private bool IsLoadingNew = false;
    [Parameter] public EventCallback<SavedEventArgs> OnSaved { get; set; }
    private string ReqId {get; set;} = string.Empty;
    
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
    
    public async Task ShowAsync(Guid id, Guid rejectionId, string reqId)
    {
        Limpiar();  
        Id = id;
        ReqId = reqId;
        RejectionId = rejectionId;
        await JS!.InvokeVoidAsync("modalHelpers.clickElement", showModalRef);
        IsLoadingData = true;
        StateHasChanged();
    }
    
    private void Limpiar()
    {
        createDto = new()
        {
            RejectionId = default
        };
        IsLoadingNew = false;
    }
    
    
    private string RejectionDateString
    {
        get => createDto.RejectionDate.ToString("yyyy-MM-dd"); // formato ISO compatible con <input type="date">
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

}