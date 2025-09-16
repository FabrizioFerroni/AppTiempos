using AppTiemposV3.SharedClases.GenericModels;
using AppTiemposV3.Web.Services;
using Microsoft.AspNetCore.Components;
using static NanoidDotNet.Nanoid;

namespace AppTiemposV3.Web.Components.UI;

public partial class ColorPicker : ComponentBase
{
    [Inject] private ColorService ColorService { get; set; } = null!;
    
    protected override async Task OnInitializedAsync()
    {
        await ColorService.InitializeAsync();
    }
    
    private async void SetColor(string name)
    {
        await ColorService.SetPreferredColorAsync(name);
        StateHasChanged();
    }
    
    private string GetButtonClass2(ColorModel color) =>
        $"w-full justify-start gap-3 {(color.Name == ColorService.PreferredColor ? color.Gradient + " text-white" : "hover:bg-gray-50 dark:hover:bg-gray-800")}";
    
    private string GetButtonClass(ColorModel? color) =>
        $"rounded-md border cursor-pointer flex items-center gap-5 w-full px-4 py-2 text-sm " +
        $"dark:text-gray-300 text-gray-500 dark:border-gray-700 " +
        $"dark:focus:bg-white/5 dark:focus:text-white focus:outline-hidden " +
        $"{(color.Name == ColorService.PreferredColor 
            ? color.Gradient + " text-white" 
            : $" {color.Hover}")}";


}