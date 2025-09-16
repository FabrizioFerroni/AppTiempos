using AppTiemposV3.Web.Services;
using Microsoft.AspNetCore.Components;

namespace AppTiemposV3.Web.Components.UI;

public partial class Navbar : ComponentBase
{
    [Parameter] public EventCallback OnToggleSidebar { get; set; }
    [Inject] private LayoutState State { get; set; } = default!;
    [Parameter] public string Title { get; set; } = string.Empty;
    [Parameter] public string Subtitle { get; set; } = string.Empty;
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Inject] private NavigationManager NavManager { get; set; } = default!;
    
    private string[] Segments { get; set; } = [];
    private string CurrentPageName { get; set; } = "Dashboard";
    
    private readonly Dictionary<string, string> RouteNames = new()
    {
        { "/dashboard", "Dashboard" },
        { "/activities", "Actividades" },
        { "/training", "Capacitaciones" },
        { "/reports", "Reportes" },
        { "/settings", "Configuración" },
        { "/profile", "Perfil" },
        { "/invitations", "Invitaciones" },
        { "/requirements", "Requerimientos" },
        { "/rejections", "Rechazos" },
        { "/weekly", "Vista Semanal" },
    };

    protected override async Task OnInitializedAsync()
    {
        await State.InitializeAsync();
    }
    
    protected override void OnInitialized()
    {
        string? uri = NavManager.ToBaseRelativePath(NavManager.Uri);
        Segments = uri.Split("/", StringSplitOptions.RemoveEmptyEntries);

        string? pathname = "/" + string.Join("/", Segments);

        // Busca nombre de ruta, si no existe usa último segmento
        if (RouteNames.TryGetValue(pathname, out var name))
        {
            CurrentPageName = name;
        }
        else if (Segments.Length > 0)
        {
            CurrentPageName = Segments[^1];
        }
    }
    
    private void ToggleSidebar()
    {
        _ = State.ToggleSidebar();
    }
}