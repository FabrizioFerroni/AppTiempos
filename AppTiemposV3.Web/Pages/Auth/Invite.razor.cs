using AppTiemposV3.SharedClases.DTOs;
using static AppTiemposV3.SharedClases.Utilidades.TokenHelper;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AppTiemposV3.Web.Pages.Auth;

public partial class Invite : ComponentBase
{
    private bool isSubmitted = false;
    private bool isLoading = false;
    private bool isError = false;
    private MarkupString messageError = new MarkupString("");
    [Inject] private IJSRuntime? Js { get; set; }
    
    private InviteDto invite = new InviteDto
    {
        FullName = null,
        Email = null,
        Reason = null
    };

    private async Task SendInvite()
    {
        isLoading = true;
        StateHasChanged();
        
        await Task.Delay(5000); // Espera 5 segundos
        
        isSubmitted = true;
        isLoading = false;
        string token = CrearToken(new { nombre = invite.FullName, email = invite.Email });
        Console.WriteLine(token);
        await Js!.InvokeVoidAsync("console.log", $"Token: {token}");
        StateHasChanged();
        
    }
}