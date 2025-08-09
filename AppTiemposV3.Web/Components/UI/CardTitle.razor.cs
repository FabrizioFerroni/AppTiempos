using Microsoft.AspNetCore.Components;
using static AppTiemposV3.Web.Utils.CssHelper;

namespace AppTiemposV3.Web.Components.UI;

public partial class CardTitle : ComponentBase
{
    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Id { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private string GetClasses()
    {
        string baseClasses =
            "text-2xl font-semibold leading-none tracking-tight";

        return Cn(baseClasses, Class);
    }
}