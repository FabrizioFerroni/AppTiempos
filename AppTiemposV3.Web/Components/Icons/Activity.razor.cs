using Microsoft.AspNetCore.Components;

namespace AppTiemposV3.Web.Components.Icons;

public partial class Activity : ComponentBase
{
    [Parameter] public string? Class { get; set; } = string.Empty;
    [Parameter] public string? Style { get; set; } = string.Empty;
    [Parameter] public string? Id { get; set; } = string.Empty;
}