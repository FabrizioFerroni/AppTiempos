using AppTiemposV3.Web.Utils;
using Microsoft.AspNetCore.Components;

namespace AppTiemposV3.Web.Components.UI
{
    public partial class JoinItem : ComponentBase
    {
        [Parameter, EditorRequired]
        public JoinDefinition Join { get; set; } = default!;

        [Parameter, EditorRequired]
        public EventCallback OnAdd { get; set; }
    }
}
