using Microsoft.AspNetCore.Components;

namespace AppTiemposV3.Web.Components.Icons;

public partial class ExternalLink : ComponentBase
{
    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Id { get; set; }
}