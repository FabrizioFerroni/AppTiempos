using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AppTiemposV3.Web.Components.UI;

public partial class UrlInputRequeriment : ComponentBase, IDisposable
{
   
    [Parameter] public string Id { get; set; } = $"paste-url-input-{Guid.NewGuid()}";
    [Parameter] public string Name { get; set; } = string.Empty;
    [Parameter] public string Placeholder { get; set; } = string.Empty;
    [Parameter] public bool Autofocus { get; set; }
    [Parameter] public string? Value { get; set; }
    [Parameter] public EventCallback<string?> ValueChanged { get; set; }
    [Inject] private IJSRuntime? JS { get; set; } = default!;
    [Parameter] public EventCallback<int> ReqIdChanged { get; set; }
    private Input? inputComponent;
    private DotNetObjectReference<UrlInputRequeriment>? objRef;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        
        if (firstRender && inputComponent is not null)
        {
            objRef ??= DotNetObjectReference.Create(this);

            await JS!.InvokeVoidAsync(
                "crmClipboard.enablePasteHandler",
                inputComponent._inputRef,
                objRef
            );
        }
    }
    
    [JSInvokable]
    public async Task OnPasteUrlAndText(string url, string text)
    {
        Value = url;
        await ValueChanged.InvokeAsync(url);

        if (!string.IsNullOrWhiteSpace(text) && text.StartsWith("ReqID"))
        {
            string numStr = text.Replace("ReqID", "").Trim();
            if (int.TryParse(numStr, out int num))
            {
                await ReqIdChanged.InvokeAsync(num);
            }
        }

        StateHasChanged();
    }

    private async Task OnInputChanged(ChangeEventArgs e)
    {
        Value = e.Value?.ToString() ?? string.Empty;
        await ValueChanged.InvokeAsync(Value);
    }
    
    public void Dispose()
    {
        objRef?.Dispose();
    }
}