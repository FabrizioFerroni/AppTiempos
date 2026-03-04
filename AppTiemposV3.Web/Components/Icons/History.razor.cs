using Microsoft.AspNetCore.Components;

namespace AppTiemposV3.Web.Components.Icons
{
    public partial class History : ComponentBase
    {
        [Parameter] public string? Class { get; set; }
        [Parameter] public string? Id { get; set; }
        [Parameter] public string? Style { get; set; }

    }
}
