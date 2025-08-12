using AppTiemposV3.SharedClases.DTOs;
using Microsoft.AspNetCore.Components;

namespace AppTiemposV3.Web.Pages.Auth;

public partial class ForgotPassword : ComponentBase
{
    private bool isSubmitted = false;
    private bool isLoading = false;
    private bool isError = false;
    private MarkupString messageError = new MarkupString("");
    
    private ForgotPasswordDto forgotPwd = new ForgotPasswordDto();
    
    private async Task SendForgotPassword()
    {
        isLoading = true;
        StateHasChanged();
        
        await Task.Delay(5000); // Espera 5 segundos
        
        isSubmitted = true;
        isLoading = false;
        StateHasChanged();
        
    }
    
}