using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using static AppTiemposV3.Web.Utils.CssHelper;

namespace AppTiemposV3.Web.Components.UI
{
    public partial class TabsTrigger : ComponentBase
    {
        [CascadingParameter] public Tabs? Tabs { get; set; }

        [Parameter] public string Value { get; set; } = default!;
        [Parameter] public bool Disabled { get; set; }
        [Parameter] public string? Class { get; set; }
        [Parameter] public string? ActiveClass { get; set; }
        [Parameter] public RenderFragment? ChildContent { get; set; }
        [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }

        private bool IsActive => Tabs?.Value == Value;

        /*private async Task OnClick()
        {
            if (!Disabled && Tabs is not null)
                await Tabs.SetValue(Value);
        }*/

        private async Task HandleClick(MouseEventArgs e)
        {
            if (Disabled || Tabs is null)
                return;

            // lógica interna (cambiar pestaña)
            await Tabs.SetValue(Value);

            // notificar al padre si existe
            if (OnClick.HasDelegate)
                await OnClick.InvokeAsync(e);
        }

        private string GetClasses()
        {
            string baseClasses =
                "cursor-pointer inline-flex h-[calc(100%-1px)] flex-1 items-center justify-center gap-1.5 rounded-md border border-transparent px-2 py-1 text-sm font-medium whitespace-nowrap transition-[color,box-shadow] focus-visible:ring-[3px] disabled:pointer-events-none disabled:opacity-50 [&_svg]:pointer-events-none [&_svg]:shrink-0 [&_svg:not([class*='size-'])]:size-4";

            string inactiveClasses = "text-muted-foreground hover:text-foreground";

            string activeClasses = ActiveClass ?? "bg-background text-foreground shadow-sm dark:border-input dark:bg-input/30";

            string finalClasses = $"{baseClasses} {(IsActive ? activeClasses : inactiveClasses )}";

            return Cn(finalClasses, Class);
        }
    }
}
