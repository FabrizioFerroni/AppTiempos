using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using static AppTiemposV3.Web.Utils.CssHelper;

namespace AppTiemposV3.Web.Components.UI;

public partial class Input : ComponentBase
{
    [Parameter] public string Id { get; set; } = string.Empty;
    [Parameter] public string Name { get; set; } = string.Empty;
    [Parameter] public string Placeholder { get; set; } = string.Empty;
    [Parameter] public string Accept { get; set; } = string.Empty;
    [Parameter] public string Autocomplete { get; set; } = string.Empty;
    [Parameter] public string Autocapitalize { get; set; } = string.Empty;

    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Type { get; set; } = "text";

    // Boolean props
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool Required { get; set; }
    [Parameter] public bool Autofocus { get; set; }
    [Parameter] public bool Readonly { get; set; }

    // Binding
    [Parameter] public string? Value { get; set; }
    [Parameter] public EventCallback<string?> ValueChanged { get; set; }

    // Events
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
    
    private string GetClasess()
    {
        string baseClasses =
            "flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-base ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium file:text-foreground placeholder:text-[hsl(var(--muted-foreground))] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-1 disabled:cursor-not-allowed disabled:opacity-50 md:text-sm";

        return Cn(baseClasses, Class);
    }
    
    private async Task OnValueChanged(ChangeEventArgs e)
    {
        Value = e.Value?.ToString();
        await ValueChanged.InvokeAsync(Value);
    }
    
    private Dictionary<string, object> GetBooleanAttributes()
    {
        var attrs = new Dictionary<string, object>();

        if (Disabled) attrs["disabled"] = true;
        if (Required) attrs["required"] = true;
        if (Autofocus) attrs["autofocus"] = true;
        if (Readonly) attrs["readonly"] = true;

        return attrs;
    }
}