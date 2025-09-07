using AppTiemposV3.Web.Authentication;

namespace AppTiemposV3.Web.Layout;

public partial class AppLayout 
{
    private async Task Logout()
    {
        CustomAuthenticationProvider? customAuthStateProvider = (CustomAuthenticationProvider)AuthStateProvider;
        await customAuthStateProvider.UpdateAuthenticationState(null!);
        Router.NavigateTo("/");
    }
}