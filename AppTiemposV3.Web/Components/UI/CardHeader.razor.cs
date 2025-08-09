using Microsoft.AspNetCore.Components;
using static AppTiemposV3.Web.Utils.CssHelper;

namespace AppTiemposV3.Web.Components.UI;

public partial class CardHeader : ComponentBase
{
    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Id { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private string GetClasses()
    {
        string baseClasses =
            "flex flex-col space-y-1.5 p-6";

        return Cn(baseClasses, Class);
    }
}