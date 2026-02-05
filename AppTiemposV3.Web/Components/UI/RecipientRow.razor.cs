using Microsoft.AspNetCore.Components;

namespace AppTiemposV3.Web.Components.UI
{
    public partial class RecipientRow: ComponentBase
    {
        [Parameter] public string Value { get; set; } = string.Empty;
        [Parameter] public EventCallback<string> ValueChanged { get; set; }
    }
}
