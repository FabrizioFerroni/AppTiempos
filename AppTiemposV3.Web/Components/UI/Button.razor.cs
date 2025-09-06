using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using static AppTiemposV3.Web.Utils.CssHelper;

namespace AppTiemposV3.Web.Components.UI;

public partial class Button : ComponentBase
{
    [Parameter] public string Variant { get; set; } = "default";
    [Parameter] public string Size { get; set; } = "default";
    [Parameter] public string? Class { get; set; }
    
    [Parameter] public string? Type { get; set; } = "button";
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    
    private string GetButtonClasses()
    {
        string variantClasses = Variant switch
        {
            "default" => "bg-primary text-white hover:bg-primary/90",
            "destructive" => "bg-red-600 text-white hover:bg-red-700",
            "outline" => "border border-gray-300 bg-white hover:bg-gray-100",
            "secondary" => "bg-gray-100 text-black hover:bg-gray-200",
            "ghost" => "hover:bg-gray-100 dark:hover:bg-gray-700 focus:ring-0 ring-0 outline-none",
            "link" => "text-blue-500 underline hover:no-underline",
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

        string baseClasses = "inline-flex items-center justify-center rounded-md font-medium transition-colors focus:outline-none focus:ring-0 focus:ring-offset-0 disabled:opacity-50 disabled:pointer-events-none";

        return Cn(baseClasses, variantClasses, sizeClasses, Class);
    }

}