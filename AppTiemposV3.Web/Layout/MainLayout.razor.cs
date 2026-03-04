using AppTiemposV3.Web.Services;
using Microsoft.AspNetCore.Components;
using static System.StringComparison;

namespace AppTiemposV3.Web.Layout;

public partial class MainLayout
{
    #region Variables
    #region Inyecciones
    [Inject] private NavigationManager Nav { get; set; } = null!;
    [Inject] LayoutState State { get; set; } = default!;
    #endregion

    private string CurrentUrl => Nav.Uri.Replace(Nav.BaseUri, "/");
    private bool IsSetupConfig = false;

    #endregion

    #region Inicializacion
    protected override Task OnInitializedAsync()
    {
        string currentUrl = CurrentUrl;
        if (currentUrl.StartsWith("/setup", OrdinalIgnoreCase))
        {
            IsSetupConfig = true;
            StateHasChanged();
        }
        return Task.CompletedTask;
    }
    #endregion
}