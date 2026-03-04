using Microsoft.AspNetCore.Components;

namespace AppTiemposV3.Web.Components.UI
{
    public partial class LoadingData : ComponentBase
    {
        [Parameter] public string Title { get; set; } = string.Empty;
        [Parameter] public string Subtitle { get; set; } = string.Empty;
    }
}
