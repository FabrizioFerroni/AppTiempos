using Microsoft.AspNetCore.Components;

namespace AppTiemposV3.Web.Components.UI;

public partial class DeleteModal : ComponentBase
{
    [Parameter] public string Title { get; set; } = string.Empty;
    [Parameter] public string Subtitle { get; set; } = string.Empty;
    [Parameter] public string Id { get; set; } = string.Empty;
    [Parameter] public Guid IdElement { get; set; } = Guid.Empty;
    
    // Nuevo: callback para avisar al padre que se confirmó el delete
    [Parameter] public EventCallback<Guid> OnConfirm { get; set; }
}