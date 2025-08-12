using System.Text.Json;
using AppTiemposV3.SharedClases.DTOs;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AppTiemposV3.Web.Pages.Auth;

public partial class TwoFactor : ComponentBase
{
    private List<string> codeValue = Enumerable.Repeat("", 6).ToList();
    private bool IsLoading = false;
    private bool IsError = false;
    private MarkupString messageError = new("");
    
    private TwoFactorDto twoFactorDto = new();
    
    [Inject] private IJSRuntime? Js { get; set; } = default!;
    [Inject] private ISessionStorageService _sessionStorageService { get; set; } = default!;
    [Inject] private NavigationManager? Router { get; set; } = default!;
    private readonly string key =  "email";
    
    protected async override Task OnInitializedAsync()
    {
        string? email = await _sessionStorageService.GetItemAsStringAsync(key) ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email))
        {
            Router!.NavigateTo("/");
        }
        else
        {
            twoFactorDto.Email = email;
            StateHasChanged();
        }
        
    }
    

    private async Task SendTwoFactor()
    {
        twoFactorDto.Code = ObtenerCodigoCompleto();
        await Js!.InvokeVoidAsync("console.log", $"Code: {twoFactorDto.Code}");
        await Js!.InvokeVoidAsync("console.log", $"TwoFactor: {JsonSerializer.Serialize(twoFactorDto)}");
        IsLoading = true;
        StateHasChanged();
        
        await Task.Delay(5000); // Espera 5 segundos
        
        IsError = true;
        messageError = (MarkupString)"El código que has enviado es incorrecto";
        IsLoading = false;
        StateHasChanged();
    }
    
    
    private string ObtenerCodigoCompleto()
    {
        return String.Join("", codeValue);
    }

}