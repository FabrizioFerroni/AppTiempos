using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.Enums;
using AppTiemposV3.Web.Services;
using static AppTiemposV3.SharedClases.Utilidades.TokenHelper;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.Web.Pages.Auth;

public partial class RegisterInvite : ComponentBase
{
    [Parameter]
    public string Token { get; set; } = string.Empty;
    
    [Inject] private IAuthContract? AuthService { get; set; }
    [Inject] private ColorService ColorService { get; set; } = null!;
    public bool IsCreated { get; set; } = false;
    public bool IsTokenInvalid { get; set; } = false;
    private bool isLoading = false;
    private bool isError = false;
    private MarkupString messageError = new MarkupString("");
    private bool showPassword = false;
    private bool showPasswordConfirm = false;
    private string Nombre { get; set; } = string.Empty;
    private string Email { get; set; } = string.Empty;
    
    [Inject] private IJSRuntime? Js { get; set; }
    
    private UserDto? register = new UserDto();
    
    private List<Areas> OptionsAreas = Enum.GetValues(typeof(Areas))
        .Cast<Areas>()
        .Where(a => a != Areas.None)
        .ToList();
    
    private Areas? AreaSeleccionadaNullable = null;
    
    private Areas AreaSeleccionada
    {
        get => AreaSeleccionadaNullable ?? default; // default es el primer valor del enum
        set => AreaSeleccionadaNullable = value;
    }
        
    private Task OnAreaSelectedChanged(Areas value)
    {
        AreaSeleccionada = value;
        register!.Area = value;
        return Task.CompletedTask;
    }
    
    protected override async Task OnInitializedAsync()
    {
        ColorService.OnColorChanged += HandleColorChanged;
    }
    
    protected override async void OnInitialized()
    {
        Dictionary<string, string>? datos = LeerToken<Dictionary<string, string>>(Token);
        
        if (datos == null || !datos.ContainsKey("nombre") || !datos.ContainsKey("email"))
        {
            IsTokenInvalid = true;
            return;
        }

        string nombre = datos["nombre"];
        string email = datos["email"];
        string expired = datos["expired"];
        
        DateTimeOffset expiredDate = DateTimeOffset.Parse(expired);

        if (DateTimeOffset.UtcNow > expiredDate.ToUniversalTime())
        {
            IsTokenInvalid = true;
            return;
        }

        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(email))
        {
            isError = true;
            messageError = (MarkupString)("El token contiene datos inválidos.");
        }

        if (!new EmailAddressAttribute().IsValid(email))
        {
            isError = true;
            messageError = (MarkupString)("El token contiene un email inválido.");
        }

        register!.FullName = nombre;
        register!.Email = email;
    }
    
    private async void HandleColorChanged()
    {
        await InvokeAsync(StateHasChanged); 
    }
    
    private async Task SendRegister()
    {
        try
        {
            isLoading = true;
            StateHasChanged();
            
            GeneralResponse? response = await AuthService!.Register(Token, register!);
            
            if (response?.Flag == true)
            {
                Nombre = register!.FullName;
                Email = register!.Email;
                register = new();
                IsCreated = true;
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