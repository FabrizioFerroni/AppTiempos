using AppTiemposV3.SharedClases.DTOs;
using static AppTiemposV3.SharedClases.Utilidades.TokenHelper;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AppTiemposV3.Web.Pages.Auth;

public partial class RegisterInvite : ComponentBase
{
    [Parameter]
    public string Token { get; set; } = string.Empty;
    
    public bool IsCreated { get; set; } = false;
    private bool isLoading = false;
    private bool isError = false;
    private MarkupString messageError = new MarkupString("");
    private bool showPassword = false;
    private bool showPasswordConfirm = false;
    
    [Inject] private IJSRuntime? Js { get; set; }
    
    private UserDto? register = new UserDto();
    
    protected async override void OnInitialized()
    {
        Dictionary<string, string>? datos = LeerToken<Dictionary<string, string>>(Token);
        string nombre = datos!["nombre"].ToString();
        string email = datos!["email"].ToString();
        register!.FullName = nombre!;
        register!.Email = email!;
        
        await Js!.InvokeVoidAsync("console.log", $"Nombre: {nombre}, Email: {email}");
        
    }
    
    private async Task SendRegister()
    {
        isLoading = true;
        StateHasChanged();
        
        await Task.Delay(5000); // Espera 5 segundos
        
        IsCreated = true;
        isLoading = false;
        StateHasChanged();
        
    }
}