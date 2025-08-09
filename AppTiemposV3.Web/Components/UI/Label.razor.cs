using Microsoft.AspNetCore.Components;
using static AppTiemposV3.Web.Utils.CssHelper;

namespace AppTiemposV3.Web.Components.UI;

public partial class Label : ComponentBase
{
    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Id { get; set; }
    [Parameter] public string? For { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private string GetClasses()
    {
        string baseClasses =
            "text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70";

        return Cn(baseClasses, Class);
    }
}