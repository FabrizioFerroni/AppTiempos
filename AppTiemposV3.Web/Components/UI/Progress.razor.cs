using AppTiemposV3.SharedClases.GenericModels;
using AppTiemposV3.Web.Services;
using Microsoft.AspNetCore.Components;
using static AppTiemposV3.Web.Utils.CssHelper;

namespace AppTiemposV3.Web.Components.UI;

public partial class Progress : ComponentBase
{
    [Parameter]
    public int Value { get; set; } = 0; // porcentaje 0-100

    [Parameter]
    public string? Class { get; set; } // clases extra para el Root

    [Parameter]
    public string? IndicatorClass { get; set; } // clases extra para el indicador
    
    [Inject] private ColorService ColorService { get; set; } = null!;
    
    protected override void OnInitialized()
    {
        ColorService.OnColorChanged += HandleColorChanged;
    }
    
    private void HandleColorChanged()
    {
        InvokeAsync(StateHasChanged); 
    }

    private string GetRootClasses()
    {
        string baseClasses =  "relative w-full overflow-hidden rounded-full bg-[hsl(var(--secondary))]";

        return Cn(baseClasses, Class);
    }

    private string GetIndicatorClasses()
    {
        //bg-[hsl(var(--primary))]
        ColorModel? currentColor = ColorService.GetCurrentColor();
        string baseClasses =  $"h-full w-full flex-1 {currentColor.Gradient}  transition-all";

        return Cn(baseClasses, IndicatorClass);
    }

    private string GetIndicatorStyle() =>
        $"transform: translateX(-{100 - Value}%);";
    
    public void Dispose()
    {
        ColorService.OnColorChanged -= HandleColorChanged; // 🧹 limpiar para evitar memory leaks
    }
}