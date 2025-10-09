using Microsoft.AspNetCore.Components;

namespace AppTiemposV3.Web.Components.UI;

public partial class Modal : ComponentBase
{
    [Parameter] public string Id { get; set; } = string.Empty;
    [Parameter] public string Modulo { get; set; } = string.Empty;
    [Parameter] public Guid IdElement { get; set; } = Guid.Empty;
    [Parameter] public string Method { get; set; } = "modal";
    [Parameter] public RenderFragment? ChildContent { get; set; }
    
    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter] public string? StylePanel { get; set; }
}