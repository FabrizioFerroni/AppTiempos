using System.Text.Json;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.Web.Authentication;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.Web.Pages.Auth;

public partial class TwoFactor : ComponentBase
{
    [Inject] private IAuthContract? AuthService { get; set; }
    [Inject] private AuthenticationStateProvider? AuthStateProvider { get; set; }
    private List<string> codeValue = Enumerable.Repeat("", 6).ToList();
    private bool IsLoading = false;
    private bool IsError = false;
    private MarkupString messageError = new("");
    
    private Login2FA login2FA = new();
    
    [Inject] private IJSRuntime? Js { get; set; } = default!;
    [Inject] private ISessionStorageService _sessionStorageService { get; set; } = default!;
    [Inject] private NavigationManager? Router { get; set; } = default!;
    private readonly string key =  "email";
    
    protected override async Task OnInitializedAsync()
    {
        string? email = await _sessionStorageService.GetItemAsync<string>(key) ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email))
        {
            Router!.NavigateTo("/");
        }
        else
        {
            login2FA.Email = email;
            StateHasChanged();
        }
        
    }
    

    private async Task SendTwoFactor()
    {
        login2FA.Code = ObtenerCodigoCompleto();
        IsLoading = true;
        StateHasChanged();
        
        try
        {
            IsLoading = true;
            StateHasChanged();
            
            LoginResponse? response = await AuthService!.Login2FA(login2FA);
            
            if (response?.Flag == true && response.TwoFa == true && response?.Token != null)
            {
                TokenDto? token = response.Token;
                string tokenLogin = token.AccessToken;
                bool rememberMe = await _sessionStorageService.GetItemAsync<bool>("remember");
                CustomAuthenticationProvider? customAuthStateProvider =
                    (CustomAuthenticationProvider)AuthStateProvider!;
                await customAuthStateProvider.UpdateAuthenticationState(tokenLogin, rememberMe);
                Router!.NavigateTo("/app");
            }
            else
            {
                IsError = true;
                messageError = (MarkupString)(response?.Message?.Replace("\n", "<br />") ?? "Error desconocido");
            }
        }
        catch (Exception ex)
        {
            IsError = true;
            this.messageError = new MarkupString(ex.Message);
        }
        finally
        {
            await _sessionStorageService.RemoveItemAsync(key);
            await _sessionStorageService.RemoveItemAsync("remember");
            IsLoading = false;
            StateHasChanged();
        }
    }
    
    
    private string ObtenerCodigoCompleto()
    {
        return String.Join("", codeValue);
    }

}