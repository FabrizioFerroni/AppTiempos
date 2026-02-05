using Microsoft.AspNetCore.Components;

namespace AppTiemposV3.Web.Components.UI
{
    public partial class Tabs : ComponentBase
    {
        [Parameter] public string? Class { get; set; }
        [Parameter] public string? Value { get; set; }
        [Parameter] public EventCallback<string> ValueChanged { get; set; }
        [Parameter] public RenderFragment? ChildContent { get; set; }

        internal async Task SetValue(string value)
        {
            Value = value;
            await ValueChanged.InvokeAsync(value);
        }
    }
}
