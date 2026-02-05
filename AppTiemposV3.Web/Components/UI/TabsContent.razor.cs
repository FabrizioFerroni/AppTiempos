using Microsoft.AspNetCore.Components;

namespace AppTiemposV3.Web.Components.UI
{
    public partial class TabsContent : ComponentBase
    {
        [CascadingParameter] public Tabs? Tabs { get; set; }

        [Parameter] public string Value { get; set; } = default!;
        [Parameter] public string? Class { get; set; }
        [Parameter] public RenderFragment? ChildContent { get; set; }

        private bool IsActive => Tabs?.Value == Value;
    }
}
