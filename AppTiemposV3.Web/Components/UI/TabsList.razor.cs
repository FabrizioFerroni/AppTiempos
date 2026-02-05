using Microsoft.AspNetCore.Components;

namespace AppTiemposV3.Web.Components.UI
{
    public partial class TabsList : ComponentBase
    {
        [Parameter] public string? Class { get; set; }
        [Parameter] public RenderFragment? ChildContent { get; set; }
    }
}
