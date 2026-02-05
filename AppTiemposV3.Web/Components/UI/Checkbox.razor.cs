using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using static AppTiemposV3.Web.Utils.CssHelper;

namespace AppTiemposV3.Web.Components.UI
{
    public partial class Checkbox : ComponentBase
    {
        [Parameter] public string Id { get; set; } = string.Empty;
        [Parameter] public string Name { get; set; } = string.Empty;
        [Parameter] public string? Class { get; set; }
        [Parameter] public string? Style { get; set; }
        [Parameter] public string Label { get; set; } = string.Empty;

        // Boolean props
        [Parameter] public bool Disabled { get; set; }
        [Parameter] public bool Required { get; set; }
        [Parameter] public bool Autofocus { get; set; }
        [Parameter] public bool Readonly { get; set; }
        //[Parameter] public bool? Checked { get; set; }
        [Parameter] public string? StyleSpan { get; set; } = "";

        // Binding bool
        [Parameter] public bool Value { get; set; }
        [Parameter] public EventCallback<bool> ValueChanged { get; set; }

        // Events
        [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }

        private async Task OnChange(ChangeEventArgs e)
        {
            if (bool.TryParse(e.Value?.ToString(), out bool newValue))
            {
                Value = newValue;
                await ValueChanged.InvokeAsync(Value);
            }
        }

        private string GetClasses()
        {
            string baseClasses =
                "appradius peer h-4 w-4 shrink-0 rounded-sm border border-primary " +
                "ring-offset-background focus-visible:outline-none " +
                "focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 " +
                "disabled:cursor-not-allowed disabled:opacity-50 " +
                "checked:bg-primary " +
                "checked:border-primary";

            return Cn(baseClasses, Class);
        }

        private Dictionary<string, object> GetBooleanAttributes()
        {
            Dictionary<string, object>? attrs = new Dictionary<string, object>();

            if (Disabled) attrs["disabled"] = true;
            if (Required) attrs["required"] = true;
            if (Autofocus) attrs["autofocus"] = true;
            if (Readonly) attrs["readonly"] = true;

            return attrs;
        }
    }
}