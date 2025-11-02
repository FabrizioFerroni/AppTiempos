using Microsoft.AspNetCore.Components;
using static AppTiemposV3.Web.Utils.CssHelper;

namespace AppTiemposV3.Web.Components.UI;

public partial class Alert : ComponentBase
{
    [Parameter] public string Variant { get; set; } = "default";
    [Parameter] public string? Class { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    
    
    private string GetClasses()
    {
        string variantClasses = Variant switch
        {
            "destructive" => "border-[hsl(var(--destructive))]/50 text-[hsl(var(--destructive))] dark:border-[hsl(var(--destructive))] [&>svg]:text-[hsl(var(--destructive))]",
            _ => "bg-[hsl(var(--background))] text-[hsl(var(--foreground))]"
        };
        
        string baseClasses =
            $"mb-1 font-medium leading-none tracking-tight relative w-full rounded-lg border p-4 [&>svg~*]:pl-7 [&>svg+div]:translate-y-[-3px] [&>svg]:absolute [&>svg]:left-4 [&>svg]:top-4 [&>svg]:text-[hsl(var(--foreground))]";

        return Cn(baseClasses, variantClasses, Class);
    }
}