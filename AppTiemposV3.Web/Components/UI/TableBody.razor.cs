using Microsoft.AspNetCore.Components;
using static AppTiemposV3.Web.Utils.CssHelper;

namespace AppTiemposV3.Web.Components.UI;

public partial class TableBody : ComponentBase
{
    [Parameter] public string? Class { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }


    protected string GetTbodyCssClass()
    {
        string baseClasses =
            "[&_tr:last-child]:border-0";

        return Cn(baseClasses, Class);

    }
}