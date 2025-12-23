using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs.Users;
using AppTiemposV3.SharedClases.Enums;
using AppTiemposV3.Web.Authentication;
using AppTiemposV3.Web.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.Web.Pages.Profile;

public partial class Index : ComponentBase, IDisposable
{

    #region Variables

    #region InyeccionDependencias
    [Inject] IUserCContract<UserResponseDto> UserService { get; set; }
    [Inject] LayoutState State { get; set; } = null!;
    [Inject] public IJSRuntime? JS { get; set; }
    [Inject] private NavigationManager? Router { get; set; }
    [Inject] private NotificationService Toltip { get; set; } = default!;
    [Inject] private ColorService ColorService { get; set; } = null!;
    [Inject] private ILocalStorageService _localStorageService { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = null!;
    [Inject] private NavigationManager Nav { get; set; } = null!;
    private string? theme = "light";

    #endregion
    private bool IsSelectClosed = true;
    private UpdateUserDto updateUser = new();
    private UpdatePasswordUserDto updatePassword = new();

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

    private bool IsLoading = false;
    private bool IsLoadingData = false;
    private string avatarUrl = string.Empty;

    private bool showPassword = false;
    private bool showPasswordNew = false;
    private bool showPasswordConfirm = false;
    private bool IsLoadingConfirm = false;
    private bool IsLoading2fa = false;
    
    private bool twoFactorEnabled = false;
    private string Gradient => ColorService!.GetCurrentColor()!.Gradient;
    
    private Guid _guidBorrar = Guid.NewGuid();

    private UserResponseDto? User { get; set; } = new UserResponseDto()
    {
        FullName = "Usuario",
        Rol = "Sin rol"
    };
    
    ElementReference fileInput;
    #endregion
    
    #region Inicializacion
    protected override async Task OnInitializedAsync()
    {
        await HandleThemeChanged();
        await JS!.InvokeVoidAsync("registerThemeChangeHandler", DotNetObjectReference.Create(this));
        ColorService.OnColorChanged += HandleColorChanged;
        State.OnSidebarChanged += StateHasChanged;
        await State.InitializeAsync();
        await GetDataAsync();
    }
    #endregion
    
    #region ObtainData

    private async Task GetDataAsync()
    {
        IsLoadingData = true;
        IsLoading2fa = true;
        StateHasChanged();

        try
        {
            DataResponse<UserResponseDto> res = await UserService.GetUserLogged();

            User = res.Data;
            FillDataCamps(res.Data);

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            IsLoadingData = false;
            IsLoading2fa = false;
            StateHasChanged();
        }
    }

    
    private void FillDataCamps(UserResponseDto userDto)
    {
        updateUser.FullName = userDto.FullName;
        updateUser.Email = userDto.Email;
        updateUser.Area = userDto.Area;
        twoFactorEnabled = userDto.TwoFactorEnable;
        OnAreaSelectedChanged(userDto.Area);
        StateHasChanged();
    }
    #endregion
    
    
    #region Properties
    private async Task HandleResolveCheck(bool check)
    {
        try
        {
            twoFactorEnabled = check;
            IsLoading2fa = true;
            StateHasChanged();

            EnableTwoFactorUser dto = new EnableTwoFactorUser()
            {
                EnableTwoFactor = check,
            };

            GeneralResponse resp = await UserService.UpdateTwoFactor(dto);

            if (resp.Flag)
            {
                Toltip.Success("Éxito!", resp.Message);
                twoFactorEnabled = dto.EnableTwoFactor;
                StateHasChanged();
            }
            else
            {
                Toltip.Error("Error", resp.Message);
            }

            StateHasChanged();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            IsLoading2fa = false;
            StateHasChanged();
        }
    }
    
    private void HandleDropdownState(bool closed)
    {
        IsSelectClosed = closed;
    }
    
    async Task OpenFileExplorer()
    {
        await JS!.InvokeVoidAsync("openFileDialog", fileInput);
    }
    
    [JSInvokable("OnThemeChanged")]
    public async Task OnThemeChanged()
    {
        await HandleThemeChanged();
    }
    
    private async Task HandleThemeChanged()
    {
        theme = await _localStorageService.GetItemAsync<string>("color-theme")!;
        StateHasChanged();
    }
    private void HandleSidebarToggle()
    {
        _ = State.ToggleSidebar();
    }
    
    private async void HandleColorChanged()
    {
        await InvokeAsync(StateHasChanged); 
    }

    #endregion
    
    #region Limpiar
    public void Dispose()
    {
        ColorService.OnColorChanged -= HandleColorChanged; 
        State.OnSidebarChanged -= StateHasChanged;
    }
    #endregion
    
    #region Functions
    private Task OnAreaSelectedChanged(Areas value)
    {
        AreaSeleccionada = value;
        updateUser!.Area = value;
        return Task.CompletedTask;
    }
    
    private string GetInitials(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return string.Empty;

        string[] parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
            return string.Empty;

        if (parts.Length == 1)
            return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();

        return $"{parts[0][0]}{parts[1][0]}".ToUpper();
    }

    public async Task SendEditUserProfile()
    {
        try
        {
            IsLoading = true;
            StateHasChanged();

            GeneralResponse resp = await UserService.UpdateUserProfile(updateUser);

            if (resp.Flag)
            {
                Toltip.Success("Éxito!", resp.Message);
                await GetDataAsync();
                StateHasChanged();
            }
            else
            {
                Toltip.Error("Error", resp.Message);
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }
    
    public async Task SendEditUserPassword()
    {
        try
        {
            IsLoadingConfirm = true;
            StateHasChanged();

            GeneralResponse resp = await UserService.UpdateUserPassword(updatePassword);

            if (resp.Flag)
            {
                Toltip.Success("Éxito!", resp.Message);
                updatePassword.ActualPassword = string.Empty;
                updatePassword.NewPassword = string.Empty;
                updatePassword.ConfirmPassword = string.Empty;
                StateHasChanged();
                await Task.Delay(5000);
                CustomAuthenticationProvider? customAuthStateProvider = (CustomAuthenticationProvider)AuthStateProvider;
                await customAuthStateProvider.UpdateAuthenticationState(null!);
                Nav.NavigateTo("/");
            }
            else
            {
                Toltip.Error("Error", resp.Message);
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            IsLoadingConfirm = false;
        }
    }
    #endregion
}