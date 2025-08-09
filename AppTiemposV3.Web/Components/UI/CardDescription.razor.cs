using Microsoft.AspNetCore.Components;
using static AppTiemposV3.Web.Utils.CssHelper;

namespace AppTiemposV3.Web.Components.UI;

public partial class CardDescription : ComponentBase
{
    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Id { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private string GetClasses()
    {
        string baseClasses =
            "text-sm text-[hsl(var(--muted-foreground))]";

        return Cn(baseClasses, Class);
    }
}