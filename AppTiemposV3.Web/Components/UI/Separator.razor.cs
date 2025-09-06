using Microsoft.AspNetCore.Components;
using static AppTiemposV3.Web.Utils.CssHelper;

namespace AppTiemposV3.Web.Components.UI;

public partial class Separator : ComponentBase
{
    [Parameter]
    public string Orientation { get; set; } = "horizontal"; // "horizontal" o "vertical"

    [Parameter]
    public bool Decorative { get; set; } = true;

    [Parameter]
    public string? Class { get; set; }

    private string Role => Decorative ? "presentation" : "separator";
    
    private string GetClasses()
    {
        string baseClasses =
            $"shrink-0 bg-red-600 {(Orientation == "horizontal" ? "h-[1px] w-full" : "h-full w-[1px]")}";

        return Cn(baseClasses, Class);
    }
}