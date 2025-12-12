using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Invitations;
using AppTiemposV3.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using static NanoidDotNet.Nanoid;
using static NanoidDotNet.Nanoid.Alphabets;
using static AppTiemposV3.Web.Utils.Helpers;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.Web.Pages.Invitations.Modales;

public partial class NewInvitationAdminModal : ComponentBase
{
    #region "Variables" 
    public Guid Id { get; set; }
    [Inject] private IJSRuntime? JS { get; set; }
    [Inject] private IInvitationContract<InvitationResponseDto> InvitationsService { get; set; } = null!;      
    [Inject] private NavigationManager? Router { get; set; }
    [Inject] private NotificationService Toltip { get; set; } = default!;
    [Inject] private ColorService ColorService { get; set; } = null!;
    private Guid InvitationId { get; set; }
    
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

    private CreateInvitationDto createDto = new()
    {
        FullName = null,
        Email = null,
        Reason = null
    };

    private bool IsLoadingNew = false;
    [Parameter] public EventCallback<SavedEventArgs> OnSaved { get; set; }
    
    private bool IsSelectClosed = true;

    private string ClassSelect => "font-medium bg-white dark:bg-gray-700 border-gray-300 dark:border-gray-600 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 dark:hover:bg-gray-500";
    #endregion
    
    public async Task ShowAsync(Guid id)
    {
        Limpiar();  
        Id = id;
        await JS!.InvokeVoidAsync("modalHelpers.clickElement", showModalRef);
        IsLoadingData = true;
        StateHasChanged();
    }
    
    private async Task SendCreateInvitation()
    {
        try
        {
            IsLoadingNew = true;
            isError = false;
            StateHasChanged();
            

            GeneralResponse response = await InvitationsService!.CreateInvitation(createDto);
            SavedEventArgs? args = new SavedEventArgs
            {
                Message = "",
                Success = false,
                StartDate = DateOnly.FromDateTime(DateTime.Now)
            };

            if (response.Flag)
            {
                await JS!.InvokeVoidAsync("modalHelpers.clickElement", closeModalRef);
                IsLoadingNew = false;
                args.Success = true;
                args.Message = response!.Message;
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
        createDto = new()
        {
            FullName = null,
            Email = null,
            Reason = null
        };
        IsLoadingNew = false;
    }
}