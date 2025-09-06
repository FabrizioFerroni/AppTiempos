using Microsoft.AspNetCore.Components;
using static AppTiemposV3.Web.Utils.CssHelper;

namespace AppTiemposV3.Web.Components.UI;

public partial class BreadcrumbEllipsis : ComponentBase
{
    [Parameter] public string? Class { get; set; }
    
    private string GetClasses()
    {
        string baseClasses =
            $"flex h-9 w-9 items-center justify-center";

        return Cn(baseClasses, Class);
    }
}