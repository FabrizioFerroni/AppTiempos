using Microsoft.AspNetCore.Components;

namespace AppTiemposV3.Web.Components.UI;

public partial class Navbar : ComponentBase
{
    [Parameter] public string Title { get; set; } = string.Empty;
    [Parameter] public string Subtitle { get; set; } = string.Empty;
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Inject] private NavigationManager NavManager { get; set; } = default!;
    
    private string[] Segments { get; set; } = Array.Empty<string>();
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
    
    protected override void OnInitialized()
    {
        var uri = NavManager.ToBaseRelativePath(NavManager.Uri);
        Segments = uri.Split("/", StringSplitOptions.RemoveEmptyEntries);

        var pathname = "/" + string.Join("/", Segments);

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
}