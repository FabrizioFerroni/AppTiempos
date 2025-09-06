using Microsoft.AspNetCore.Components;
using static AppTiemposV3.Web.Utils.CssHelper;

namespace AppTiemposV3.Web.Components.UI;

public partial class Badge : ComponentBase
{
    [Parameter] public string Variant { get; set; } = "default";
    [Parameter] public string Size { get; set; } = "default";
    [Parameter] public string? Class { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    
    private string GetClasses()
    {
        string variantClasses = Variant switch
        {
            "default" => "border-transparent bg-primary text-primary-foreground hover:bg-primary/80",
            "destructive" => "border-transparent bg-destructive text-destructive-foreground hover:bg-destructive/80",
            "outline" => "text-foreground",
            "secondary" => "border-transparent bg-secondary text-secondary-foreground hover:bg-secondary/80",
            _ => ""
        };

        string sizeClasses = Size switch
        {
            "default" => "h-10 px-4 py-2 text-sm",
            "sm" => "h-9 px-3 text-sm",
            "lg" => "h-11 px-8 text-base",
            "icon" => "h-10 w-10 p-0",
            _ => ""
        };
        
        string baseClasses =
            $"inline-flex items-center rounded-full border px-2.5 py-0.5 text-xs font-semibold transition-colors focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2";

        return Cn(baseClasses, variantClasses, Class);
    }
}
