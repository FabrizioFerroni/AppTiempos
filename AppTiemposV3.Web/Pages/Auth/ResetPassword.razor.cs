using AppTiemposV3.SharedClases.DTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AppTiemposV3.Web.Pages.Auth;

public partial class ResetPassword : ComponentBase
{
    [Parameter]
    public string Token { get; set; } = string.Empty;
    
    public bool IsSubmitted { get; set; } = false;
    private bool IsLoading = false;
    private bool IsTokenValidAndNotExpired = true;
    private bool IsError = false;
    private MarkupString messageError = new MarkupString("");
    private bool ShowPassword = false;
    private bool ShowPasswordConfirm = false;
    
    [Inject] private IJSRuntime? Js { get; set; }
    
    private ResetPasswordDto resetPwd = new ResetPasswordDto();
    
    private async Task SendResetPassword()
    {
        IsLoading = true;
        StateHasChanged();
        
        await Task.Delay(5000); // Espera 5 segundos
        
        IsSubmitted = true;
        IsLoading = false;
        StateHasChanged();
        
    }

}