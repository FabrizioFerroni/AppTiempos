using System.Text.RegularExpressions;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.Web.Pages.Auth;

public partial class ForgotPassword : ComponentBase
{
    [Inject] private IJSRuntime? Js { get; set; }
    [Inject] private IAuthContract? AuthService { get; set; }
    [Inject] private ColorService ColorService { get; set; } = null!;
    
    private bool isSubmitted = false;
    private bool isLoading = false;
    private bool isError = false;
    private MarkupString messageError = new MarkupString("");
    
    private ForgotPasswordDto forgotPwd = new ForgotPasswordDto();
    
    protected override async Task OnInitializedAsync()
    {
        ColorService.OnColorChanged += HandleColorChanged;
    }
    
    private async void HandleColorChanged()
    {
        await InvokeAsync(StateHasChanged); 
    }
    
    private async Task SendForgotPassword()
    {
        try
        {
            isLoading = true;
            StateHasChanged();
            
            GeneralResponse? response = await AuthService!.ForgotPassword(forgotPwd);
            
            if (response?.Flag == true)
            {
                forgotPwd = new();
                isSubmitted = true;
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