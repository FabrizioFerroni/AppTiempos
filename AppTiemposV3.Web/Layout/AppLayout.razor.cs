using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.Web.Layout;

public partial class AppLayout : IDisposable
{
    [Inject] LayoutState State { get; set; } = default!;
    [Inject] private IConfigurationContract ConfigurationContract { get; set; } = null!;
    [Inject] private NavigationManager Nav { get; set; } = null!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        State.OnSidebarChanged += StateHasChanged;
        await State.InitializeAsync();

        AuthenticationState? authState = await AuthStateProvider.GetAuthenticationStateAsync();
        ClaimsPrincipal? user = authState.User;

        if (user.Identity is { IsAuthenticated: true })
        {
            await GetHasConfiguration();
        }
        //await GetHasConfiguration();
    }


    private async Task GetHasConfiguration()
    {
        try
        {
            DataResponse<bool> response = await ConfigurationContract.HasConfiguration();

            if (response.Success)
            {
                if (!response.Data)
                {
                    Nav.NavigateTo("/setup");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw ex;
        }
    }

    public void Dispose()
    {
        State.OnSidebarChanged -= StateHasChanged;
    }
}