using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.Web.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.Web.Pages.Auth;

public partial class Login : ComponentBase
{
    [Inject] private IJSRuntime? Js { get; set; }
    [Inject] private NavigationManager? Router { get; set; }
    [Inject] private AuthenticationStateProvider? AuthStateProvider { get; set; }
    [Inject] private IAuthContract? AuthService { get; set; }
    
    private bool isLoading = false;
    private bool isError = false;
    private bool showPassword = false;
    private MarkupString messageError;

    private LoginDto login = new LoginDto();
    
    private async Task SendLogin()
    {
        try
        {
            await Js!.InvokeVoidAsync("console.log", $"Email: {login.Email}, Password: {login.Password}");
            await Js!.InvokeVoidAsync("console.log", "Login function called!");

            isLoading = true;
            StateHasChanged();

           LoginResponse? response = await AuthService!.Login(login, "");

            if (response?.Flag == true && response?.Token != null)
            {
                TokenDto? token = response.Token;
                bool rememberMe = login.RememberMe;
                string tokenLogin = token.AccessToken;
                login = new();

                CustomAuthenticationProvider? customAuthStateProvider = (CustomAuthenticationProvider)AuthStateProvider!;
                await customAuthStateProvider.UpdateAuthenticationState(tokenLogin, rememberMe);
                Router!.NavigateTo("/app");
            }
            else
            {
                isError = true;
                messageError = (MarkupString)(response?.Message?.Replace("\n", "<br />") ?? "Error desconocido");
            }
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }
}