using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;

namespace AppTiemposV3.Web.Pages;

public partial class Home: ComponentBase
{
    [Inject] private ILocalStorageService LocalStorage {
        get;
        set; 
    } = default!;
    
    private bool toggleDark = false;
    
    private async Task ChangeTheme()
    {
        toggleDark = !toggleDark;
        string? theme = await LocalStorage.GetItemAsStringAsync("theme");

        if (theme == "dark")
        {
            await LocalStorage.SetItemAsStringAsync("theme", "light");
        }
        else
        {
            await LocalStorage.SetItemAsStringAsync("theme", "dark");
        }
    }
}