using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Invitations;
using AppTiemposV3.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using static NanoidDotNet.Nanoid;
using static NanoidDotNet.Nanoid.Alphabets;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.Web.Pages.Invitations.Modales;

public partial class DetailsInvitationModal : ComponentBase
{
    #region Variables
    public Guid Id { get; set; }
    [Inject] private IJSRuntime? JS { get; set; }
    [Inject] private IInvitationContract<InvitationResponseDto> InvitationsService { get; set; } = null!;   
    [Inject] private ColorService ColorService { get; set; } = null!;
    private Dictionary<string, (string Mensaje, bool EsExitoso)> mensajes = new();
    public bool IsLoadingData { get; set; } = true;
    private string IdModalStr = Generate(LowercaseLettersAndDigits, 10);
    [Parameter] public EventCallback<SavedEventArgs> OnSaved { get; set; }
    private InvitationResponseDto Invitation  { get; set; } = null!;
    private ElementReference showModalRef;
    private ElementReference closeModalRef;
    
    private bool IsLoadingA = false;
    private bool IsLoadingD = false;
    #endregion
    
    
    public async Task ShowAsync(Guid id)
    {
        Id = id;
        await JS!.InvokeVoidAsync("modalHelpers.clickElement", showModalRef);
        IsLoadingData = true;
        StateHasChanged();
        await LoadDataReq(id);
    }
    
    private async Task LoadDataReq(Guid id)
    {
        IsLoadingData = true;
        try
        {
            DataResponse<InvitationResponseDto>? invitation =
                await InvitationsService.GetInvitationPorId(id);

            Invitation = invitation.Data;
            mensajes = new Dictionary<string, (string Mensaje, bool EsExitoso)>();
            StateHasChanged();

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            IsLoadingData = false;
            StateHasChanged();
        }
    }


    private async Task AcceptOrDeclineInvitation(bool result)
    {
        try
        {
            if (result)
            {
                IsLoadingA = true;
            }
            else
            {
                IsLoadingD = true;
            }

            StateHasChanged();
            AcceptOrDeclineInvitationDto dto = new AcceptOrDeclineInvitationDto()
            {
                AcceptDecline = result
            };
            
            GeneralResponse response = await InvitationsService!.AcceptOrDeclineInvitation(Id, dto);
            SavedEventArgs? args = new SavedEventArgs
            {
                Message = "",
                Success = false,
                StartDate = DateOnly.FromDateTime(DateTime.Now)
            };

            if (response.Flag)
            {
                await JS!.InvokeVoidAsync("modalHelpers.clickElement", closeModalRef);
                args.Success = true;
                if (result)
                {
                    IsLoadingA = false;
                    args.Message = "Se ha aprobado con éxito a la invitación. Se le enviara un mail con los pasos a seguir.";
                }
                else
                {
                    args.Message = "Se ha denegado con éxito a la invitación";
                    IsLoadingD = false;
                }
                await OnSaved.InvokeAsync(args);
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            if (result)
            {
                IsLoadingA = false;
            }
            else
            {
                IsLoadingD = false;
            }
            StateHasChanged();
        }
    }
}