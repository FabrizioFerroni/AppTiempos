using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using static AppTiemposV3.Web.Utils.CssHelper;

namespace AppTiemposV3.Web.Components.UI;

public partial class Input2Fa : ComponentBase
{
    [Inject] private IJSRuntime? Js { get; set; }
    
    [Parameter]
    public List<string> Code { get; set; } = Enumerable.Repeat("", 6).ToList();
    [Parameter]
    public EventCallback<List<string>> CodeChanged { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter] public int? MaxLength { get; set; }
    [Parameter] public int? Min { get; set; }
    
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
    
    [Parameter(CaptureUnmatchedValues = true)] 
    public Dictionary<string, object>? AdditionalAttributes { get; set; }
    
    private string GetClasess()
    {
        string baseClasses =
            "flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-base ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium file:text-foreground placeholder:text-[hsl(var(--muted-foreground))] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-1 disabled:cursor-not-allowed disabled:opacity-50 md:text-sm";

        return Cn(baseClasses, Class);
    }
    
    private async Task OnValueChanged(int index, ChangeEventArgs e)
    {
        string val = e.Value?.ToString() ?? "";
        if (index >= 0 && index < Code.Count)
        {
            Code[index] = val;
            await CodeChanged.InvokeAsync(Code);

            if (!string.IsNullOrEmpty(val) && index < Code.Count - 1)
            {
                await FocusInput(index + 1);
            }
        }
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
    
    
    
    private async void HandleInputChange(int index, ChangeEventArgs e)
    {
        if (index >= 0 && index < Code.Count)
        {
            Code[index] = e.Value?.ToString() ?? "";
            
            if (!string.IsNullOrEmpty(e.Value?.ToString()) && index < Code.Count - 1)
            {
                await FocusInput(index + 1);
            }
        }
    }

    private async void HandleKeyDown(int index, KeyboardEventArgs e)
    {
        if (index < 0 || index >= Code.Count) return;
        
        if (e.Key == "Backspace")
        {
            if (string.IsNullOrEmpty(Code[index]) && index > 0)
            {
                await FocusInput(index - 1);
            }
            else
            {
                Code[index] = ""; 
            }
        }
    }
    
    private async Task FocusInput(int index)
    {
        await Js!.InvokeVoidAsync("document.getElementById", $"digit-{index}").AsTask();
        await Js!.InvokeVoidAsync("eval", $"document.getElementById('digit-{index}').focus()");
    }
}