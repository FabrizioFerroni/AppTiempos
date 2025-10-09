using Microsoft.AspNetCore.Components;

namespace AppTiemposV3.Web.Components.UI;

public partial class LoadingModal : ComponentBase
{
    // [Parameter] public string Id { get; set; } = string.Empty;
    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public string Modulo { get; set; } = string.Empty;
    // [Parameter] public Guid IdElement { get; set; } = Guid.Empty;
}