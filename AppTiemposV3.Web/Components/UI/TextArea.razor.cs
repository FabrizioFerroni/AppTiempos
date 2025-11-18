using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace AppTiemposV3.Web.Components.UI;

public partial class TextArea : ComponentBase
{
    [Parameter] public string Id { get; set; } = string.Empty;
    [Parameter] public string Name { get; set; } = string.Empty;
    [Parameter] public string Placeholder { get; set; } = string.Empty;
    [Parameter] public string Autocomplete { get; set; } = string.Empty;
    [Parameter] public string Autocapitalize { get; set; } = string.Empty;

    [Parameter] public string? Class { get; set; }

    // Boolean props
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool Required { get; set; }
    [Parameter] public bool Autofocus { get; set; }
    [Parameter] public bool Readonly { get; set; }
    [Parameter] public string? Rows { get; set; }


    // Binding
    [Parameter] public string? Value { get; set; }
    [Parameter] public EventCallback<string?> ValueChanged { get; set; }

    // Events
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }

    private string GetClasess()
    {
        string baseClasses =
            "flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-base ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 md:text-sm ";


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

    // Método auxiliar para concatenar clases (reemplaza Tailwind Merge si lo usas)
    private string Cn(string baseClasses, string? extraClasses)
        => string.IsNullOrWhiteSpace(extraClasses)
            ? baseClasses
            : $"{baseClasses} {extraClasses}";
}