using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.Web.Authentication;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.Web.Pages.Auth;

public partial class Login : ComponentBase
{
    [Inject] private IJSRuntime? Js { get; set; }
    [Inject] private NavigationManager? Router { get; set; }
    [Inject] private AuthenticationStateProvider? AuthStateProvider { get; set; }
    [Inject] private IAuthContract? AuthService { get; set; }
    [Inject] private ISessionStorageService _sessionStorageService { get; set; } = default!;
    
    private bool isLoading = false;
    private bool isError = false;
    private bool showPassword = false;
    private MarkupString messageError;
    private readonly string key =  "email";
    
    private LoginDto login = new LoginDto();
    
    protected override async Task OnInitializedAsync()
    {
        bool containEmail = await _sessionStorageService.ContainKeyAsync(key);
        if (containEmail)
        {
            await _sessionStorageService.RemoveItemAsync(key);
        }
        bool containRemember = await _sessionStorageService.ContainKeyAsync("remember");
        if (containRemember)
        {
            await _sessionStorageService.RemoveItemAsync("remember");
        }
            
    }
    
    private async Task SendLogin()
    {
        try
        {
            isLoading = true;
            StateHasChanged();

            LoginResponse? response = await AuthService!.Login(login, "");

            bool rememberMe = login.RememberMe;
            if (response?.Flag == true && response.TwoFa == false && response?.Token != null)
            {
                TokenDto? token = response.Token;
                string tokenLogin = token.AccessToken;
                login = new();

                CustomAuthenticationProvider? customAuthStateProvider =
                    (CustomAuthenticationProvider)AuthStateProvider!;
                await customAuthStateProvider.UpdateAuthenticationState(tokenLogin, rememberMe);
                Router!.NavigateTo("/app");
            }
            else if (response?.Flag == true && response.TwoFa == true)
            {
                await _sessionStorageService.SetItemAsync(key, login.Email);
                await _sessionStorageService.SetItemAsync("remember", rememberMe);
                Router!.NavigateTo("/2fa");
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
    
    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        // Regex simple de email
        return Regex.IsMatch(email,
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.IgnoreCase);
    }
}