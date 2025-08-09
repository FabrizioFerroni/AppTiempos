using Microsoft.AspNetCore.Components;
using static AppTiemposV3.Web.Utils.CssHelper;

namespace AppTiemposV3.Web.Components.UI;

public partial class CardContent : ComponentBase
{
    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Id { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private string GetClasses()
    {
        string baseClasses =
            "p-6 pt-0";

        return Cn(baseClasses, Class);
    }
}