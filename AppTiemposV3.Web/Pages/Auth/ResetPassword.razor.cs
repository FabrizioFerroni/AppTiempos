using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.SharedClases.Utilidades.TokenHelper;

namespace AppTiemposV3.Web.Pages.Auth;

public partial class ResetPassword : ComponentBase
{
    [Parameter]
    public string Token { get; set; } = string.Empty;
    
    [Inject] private IAuthContract? AuthService { get; set; }
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private ColorService ColorService { get; set; } = null!;
    public bool IsSubmitted { get; set; } = false;
    private bool IsLoading = false;
    private bool IsTokenValidAndNotExpired = true;
    private bool IsError = false;
    private MarkupString messageError = new MarkupString("");
    private bool ShowPassword = false;
    private bool ShowPasswordConfirm = false;
    
    [Inject] private IJSRuntime? Js { get; set; }
    
    private ResetPasswordDto resetPwd = new ResetPasswordDto();
    
    private int segundos = 30;
    private Timer? timer;

    protected override async Task OnInitializedAsync()
    {
        ColorService.OnColorChanged += HandleColorChanged;
    }
    
    protected override async void OnInitialized()
    {
        Dictionary<string, string>? datos = LeerToken<Dictionary<string, string>>(Token);
        
        if (datos == null || !datos.ContainsKey("email"))
        {
            IsTokenValidAndNotExpired = true;
            return;
        }

        string email = datos["email"];
        string expired = datos["expired"];
        
        DateTimeOffset expiredDate = DateTimeOffset.Parse(expired);

        if (DateTimeOffset.UtcNow > expiredDate.ToUniversalTime())
        {
            IsTokenValidAndNotExpired = false;
            return;
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            IsError = true;
            messageError = (MarkupString)("El token contiene datos inválidos.");
        }

        if (!new EmailAddressAttribute().IsValid(email))
        {
            IsError = true;
            messageError = (MarkupString)("El token contiene un email inválido.");
        }
    }
    
    private async void HandleColorChanged()
    {
        await InvokeAsync(StateHasChanged); 
    }
    
    private async Task SendResetPassword()
    {
        try
        {
            IsLoading = true;
            StateHasChanged();
            
            GeneralResponse? response = await AuthService!.ResetPassword(Token, resetPwd);
            
            if (response?.Flag == true)
            {

                resetPwd = new();
                IsSubmitted = true;
                IsLoading = false;
                timer = new Timer(async _ =>
                {
                    if (segundos > 0)
                    {
                        segundos--;
                        await InvokeAsync(StateHasChanged);
                    }
                    else
                    {
                        timer?.Dispose();
                        NavigationManager nav = Navigation; 
                        nav.NavigateTo("/");
                    }
                }, null, 1000, 1000);
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
            IsLoading = false;
            StateHasChanged();
        }
        
    }
    
    private bool IsValidPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;
        
        return Regex.IsMatch(password,
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$");
    }

    public void Dispose()
    {
        ColorService.OnColorChanged -= HandleColorChanged; 
    }
}