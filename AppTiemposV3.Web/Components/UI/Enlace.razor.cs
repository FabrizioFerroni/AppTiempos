using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using static AppTiemposV3.Web.Utils.CssHelper;

namespace AppTiemposV3.Web.Components.UI;

public partial class Enlace : ComponentBase
{
    [Parameter] public string? Class { get; set; }
    [Parameter] public string Variant { get; set; } = "";
    [Parameter] public string? Id { get; set; }
    [Parameter] public string? Target { get; set; }
    [Parameter] public string? Rel { get; set; }
    [Parameter] public string? Href { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }

    private string GetClasses()
    {
        string variantClasses = Variant switch
        {
            "default" => "bg-primary text-white hover:bg-primary/90",
            "destructive" => "bg-red-600 text-white hover:bg-red-700",
            "outline" => "border border-gray-300 bg-white hover:bg-gray-100",
            "secondary" => "bg-gray-100 text-black hover:bg-gray-200",
            "ghost" => "hover:bg-gray-100",
            "link" => "text-blue-500 underline hover:no-underline",
            _ => ""
        };
        
        string baseClasses =
            "";

        return Cn(baseClasses, variantClasses, Class);
    }
}