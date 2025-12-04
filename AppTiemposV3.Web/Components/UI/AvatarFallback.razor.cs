using Microsoft.AspNetCore.Components;

namespace AppTiemposV3.Web.Components.UI;

public partial class AvatarFallback : ComponentBase
{
    [Parameter] public string? Class { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }
}