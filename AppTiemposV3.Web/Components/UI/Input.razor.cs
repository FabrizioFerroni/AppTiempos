using static System.Text.RegularExpressions.Regex;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using static AppTiemposV3.Web.Utils.CssHelper;

namespace AppTiemposV3.Web.Components.UI;

public partial class Input : ComponentBase
{
    public ElementReference _inputRef;
    [Parameter] public string Id { get; set; } = string.Empty;
    [Parameter] public string Name { get; set; } = string.Empty;
    [Parameter] public string Placeholder { get; set; } = string.Empty;
    [Parameter] public string Accept { get; set; } = string.Empty;
    [Parameter] public string Autocomplete { get; set; } = string.Empty;
    [Parameter] public string Autocapitalize { get; set; } = string.Empty;

    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter] public string? Type { get; set; } = "text";

    [Parameter] public int? MaxLength { get; set; }
    [Parameter] public int? MinLength { get; set; }
    [Parameter] public int? Min { get; set; }
    [Parameter] public int? Max { get; set; }
    
    [Parameter] public string? Pattern { get; set; }
    [Parameter] public string? InputMode { get; set; }
    
    // Boolean props
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool Required { get; set; }
    [Parameter] public bool Autofocus { get; set; }
    [Parameter] public bool Readonly { get; set; }
    [Parameter] public bool DigitsOnly { get; set; } = false;
    

    // Binding
    [Parameter] public string? Value { get; set; }
    [Parameter] public EventCallback<string?> ValueChanged { get; set; }

    // Events
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
    
    [Parameter(CaptureUnmatchedValues = true)] 
    public Dictionary<string, object>? AdditionalAttributes { get; set; }
    
    /*private string CurrentValue
    {
        get => Value ?? string.Empty;
        set
        {
            string? incoming = value ?? string.Empty;

            if (DigitsOnly)
            {
                incoming = Regex.Replace(incoming, @"[^0-9\.]", "");
            }

            // Solo propagar si cambió (evita loops)
            if (incoming != Value)
            {
                Value = incoming;
                // no await para evitar reentradas en el setter
                _ = ValueChanged.InvokeAsync(Value);
            }
        }
    }*/
    
    private string CurrentValue
    {
        get => Value ?? string.Empty;
        set
        {
            string incoming = value ?? string.Empty;

            if (DigitsOnly)
            {
                incoming = Replace(incoming, @"[^0-9\.]", "");
            }

            // Evita bucles y cambios innecesarios
            if (!string.Equals(Value, incoming, StringComparison.Ordinal))
            {
                Value = incoming;
                ValueChanged.InvokeAsync(Value);
            }
        }
    }

    
    private string GetClasess()
    {
        string baseClasses =
            "flex h-10 w-full rounded-md border border-[hsl(var(--input))] bg-[hsl(var(--background))] px-3 py-2 text-[hsl(var(--base))] ring-offset-[hsl(var(--background))] file:border-0 file:bg-[hsl(var(--transparent))] file:text-sm file:font-medium file:text-[hsl(var(--foreground))] placeholder:text-[hsl(var(--muted-foreground))] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-1 disabled:cursor-not-allowed disabled:opacity-50 md:text-sm";

        return Cn(baseClasses, Class);
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