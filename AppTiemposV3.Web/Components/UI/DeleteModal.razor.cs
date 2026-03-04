using Microsoft.AspNetCore.Components;

namespace AppTiemposV3.Web.Components.UI;

public partial class DeleteModal : ComponentBase
{
    [Parameter] public string Title { get; set; } = string.Empty;
    [Parameter] public string TitleBtn { get; set; } = "Eliminar";
    [Parameter] public string? Subtitle { get; set; } = string.Empty;
    [Parameter] public RenderFragment? SubtitleHtml { get; set; }
    [Parameter] public string Id { get; set; } = string.Empty;
    [Parameter] public Guid IdElement { get; set; } = Guid.Empty;
    [Parameter] public string Method { get; set; } = "delete";
    
    // Nuevo: callback para avisar al padre que se confirmó el delete
    [Parameter] public EventCallback<Guid> OnConfirm { get; set; }
}