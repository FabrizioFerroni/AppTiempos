using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AppTiemposV3.Web.Components.UI;

public partial class ThemeToggle : ComponentBase
{
    private string Theme { get; set; } = "system";

    [Inject] private ILocalStorageService _localStorageService { get; set; } = default!;
    
    [Inject] private IJSRuntime Js { get; set; } = null!;
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Obtener el tema guardado en LocalStorage
            string? savedTheme = await _localStorageService.GetItemAsync<string>("theme");

            if (!string.IsNullOrEmpty(savedTheme))
            {
                Theme = savedTheme;
            }

            // Aplicar el tema
            await ApplyTheme(Theme);
        }
    }

    /*private async Task ChangeTheme(string theme)
    {
        Theme = theme;
        
        await _localStorageService.SetItemAsync("theme", theme);

        if (theme == "system")
        {
            await Js.InvokeVoidAsync("eval", @"
                if(window.matchMedia('(prefers-color-scheme: dark)').matches) {
                    document.documentElement.classList.add('dark');
                } else {
                    document.documentElement.classList.remove('dark');
                }");
        }
        else if (theme == "dark")
        {
            await Js.InvokeVoidAsync("eval", "document.documentElement.classList.add('dark');");
        }
        else
        {
            await Js.InvokeVoidAsync("eval", "document.documentElement.classList.remove('dark');");
        }
    }*/
    
    private async Task ChangeTheme(string theme)
    {
        Theme = theme;

        await _localStorageService.SetItemAsync("theme", theme);

        await ApplyTheme(theme);
    }
    
    private async Task ApplyTheme(string theme)
    {
        await Js.InvokeVoidAsync("console.log", theme);
    }
}