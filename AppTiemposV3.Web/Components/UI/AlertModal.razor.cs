using Microsoft.AspNetCore.Components;

namespace AppTiemposV3.Web.Components.UI
{
    public partial class AlertModal<T> : ComponentBase
    {
        [Parameter] public string Title { get; set; } = string.Empty;
        [Parameter] public string TitleBtn { get; set; } = "Aceptar";
        [Parameter] public string TitleCancelarBtn { get; set; } = "Cancelar";
        [Parameter] public string? Subtitle { get; set; } = string.Empty;
        [Parameter] public string CssColorButtonOk { get; set; } = string.Empty;
        [Parameter] public RenderFragment? SubtitleHtml { get; set; }
        [Parameter] public string Id { get; set; } = string.Empty;
        [Parameter] public Guid IdElement { get; set; } = Guid.Empty;
        [Parameter] public string Method { get; set; } = "alert";

        [Parameter] public T? Item { get; set; }
        [Parameter] public EventCallback<T> OnConfirm { get; set; }
        [Parameter] public EventCallback<bool> OnCancel { get; set; }
    }
}
