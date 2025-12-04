using Microsoft.AspNetCore.Components;

namespace AppTiemposV3.Web.Components.UI;

public partial class AvatarImage : ComponentBase
{
    [Parameter] public string? Class { get; set; }
    [Parameter] public string Src { get; set; } = "";
    [Parameter] public string Alt { get; set; } = "";
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }
}