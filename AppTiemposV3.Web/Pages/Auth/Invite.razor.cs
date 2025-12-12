using System.Text.RegularExpressions;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Invitations;
using AppTiemposV3.Web.Services;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AppTiemposV3.Web.Pages.Auth;

public partial class Invite : ComponentBase
{
    [Inject] private IInvitationContract<InvitationResponseDto> InvitationService { get; set; } = null!;
    [Inject] private ColorService ColorService { get; set; } = null!;
    private bool isSubmitted = false;
    private bool isLoading = false;
    private bool isError = false;
    private string Email { get; set; } = string.Empty;
    private MarkupString messageError = new MarkupString("");
    [Inject] private IJSRuntime? Js { get; set; }
    
    private CreateInvitationDto invite = new ()
    {
        FullName = null,
        Email = null,
        Reason = null
    };
    
    protected override async Task OnInitializedAsync()
    {
        ColorService.OnColorChanged += HandleColorChanged;
    }
    
    private async void HandleColorChanged()
    {
        await InvokeAsync(StateHasChanged); 
    }

    private async Task SendInvite()
    {
        try
        {
            isLoading = true;
            StateHasChanged();
            
            GeneralResponse? response = await InvitationService!.CreateInvitation(invite);
            
            if (response?.Flag == true)
            {
                Email  = invite.Email;
                invite = new()
                {
                    FullName = null,
                    Email = null,
                    Reason = null
                };
                isSubmitted = true;
                isLoading = false;
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
            this.messageError = new MarkupString(ex.Message);
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
        
    }
    
    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        // Regex simple de email
        return Regex.IsMatch(email,
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.IgnoreCase);
    }
    
    public void Dispose()
    {
        ColorService.OnColorChanged -= HandleColorChanged; 
    }
}