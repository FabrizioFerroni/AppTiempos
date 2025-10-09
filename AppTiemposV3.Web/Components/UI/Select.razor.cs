using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using static AppTiemposV3.Web.Utils.CssHelper;
using static NanoidDotNet.Nanoid;

namespace AppTiemposV3.Web.Components.UI;

public partial class Select<TItem> : ComponentBase 
{
    [Inject] private IJSRuntime JS { get; set; } = default!;
    
    [Parameter] 
    public EventCallback<TItem> OnSelectedItemChanged { get; set; }
    
    private readonly string elementId = $"select-{Generate(Alphabets.LowercaseLettersAndDigits, 10)}";
    [Parameter] public IEnumerable<TItem>? Items { get; set; }
    
    private IEnumerable<TItem>? FilteredItems =>
        string.IsNullOrWhiteSpace(searchText)
            ? Items
            : Items?.Where(i => ItemTextSelector(i)
                .Contains(searchText, StringComparison.OrdinalIgnoreCase));
    [Parameter] public Func<TItem, string> ItemTextSelector { get; set; } = x => x?.ToString() ?? "";
    [Parameter] public TItem? Value { get; set; }
    [Parameter] public EventCallback<TItem?> ValueChanged { get; set; }
    [Parameter] public string? Placeholder { get; set; }
    
    [Parameter] public string? ClassButton { get; set; }
    [Parameter] public string? ClassOption { get; set; }
    [Parameter] public string? Id { get; set; }

    private bool IsOpen { get; set; }
    
    [Parameter] public bool EnableSearch { get; set; } = false;
    [Parameter] public string HeightClass { get; set; } = "max-h-96";

    private string searchText = "";
    
    [Parameter]
    public EventCallback<bool> OnDropdownStateChanged { get; set; }
    private string? SelectedText => Value != null ? ItemTextSelector(Value) : null;
    
    private DotNetObjectReference<Select<TItem>>? selfRef;
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            selfRef = DotNetObjectReference.Create(this);
            Console.WriteLine(elementId);
            await JS.InvokeVoidAsync("selectHelper.register", selfRef, elementId);
        }
    }

    private async Task Toggle()
    {
        IsOpen = !IsOpen;

        if (IsOpen)
        {
            selfRef = DotNetObjectReference.Create(this);
            await JS.InvokeVoidAsync("selectHelper.register", selfRef, elementId);
        }
        else
        {
            await JS.InvokeVoidAsync("selectHelper.unregister");
            selfRef?.Dispose();
            selfRef = null;
        }
        
        if (OnDropdownStateChanged.HasDelegate)
        {
            await OnDropdownStateChanged.InvokeAsync(!IsOpen); // true = cerrado
        }
    }
    
    private async Task SelectItem(TItem item)
    {
        Value = item;
        await ValueChanged.InvokeAsync(item);
        IsOpen = false;
        Close(); // usa tu lógica centralizada de cerrar
        searchText = string.Empty;
    }
    
    private string GetClasessButton()
    {
        string baseClasses =
            "flex h-10 w-full items-center justify-between rounded-md border border-[hsl(var(--input))] bg-[hsl(var(--background))] px-3 py-2 text-sm focus:outline-none disabled:cursor-not-allowed disabled:opacity-50  cursor-pointer";

        return Cn(baseClasses, ClassButton);
    }
    
    private async Task OnSearchChanged(ChangeEventArgs e)
    {
        searchText = e.Value?.ToString() ?? "";
        StateHasChanged(); // fuerza re-render
    }
    
    private string GetClasessOption(bool isFirst = false, bool isLast = false)
    {
        string baseClasses =
            "relative flex w-full cursor-pointer select-none items-center py-1.5 pl-8 pr-2 text-sm hover:bg-[hsl(var(--accent))] hover:text-[hsl(var(--accent-foreground))]";
        
        if (isFirst && !EnableSearch)
            baseClasses += " rounded-t-md";
        if (isLast && !EnableSearch)
            baseClasses += " rounded-b-md";


        return Cn(baseClasses, ClassOption);
    }
    
    private async void Close()
    {
        IsOpen = false;
        await JS.InvokeVoidAsync("selectHelper.unregister");
        selfRef?.Dispose();
        StateHasChanged();
    }
    
    [JSInvokable]
    public void OnEscapePressed()
    {
        IsOpen = false;
        StateHasChanged();
    }

    [JSInvokable]
    public void OnOutsideClick()
    {
        IsOpen = false;
        StateHasChanged();
    }

    public async ValueTask DisposeAsync()
    {
        if (selfRef != null)
        {
            await JS.InvokeVoidAsync("selectHelper.unregister");
            selfRef.Dispose();
        }
    }
}