using System.Net.Http.Json;
using AppTiemposV3.SharedClases.GenericModels;
using Microsoft.JSInterop;
using static NanoidDotNet.Nanoid;

namespace AppTiemposV3.Web.Services;

public class ColorService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly HttpClient _httpClient;
    private const string LocalStorageKey = "color-accent";
    public event Action? OnColorChanged;

    public List<ColorModel>? Colors { get; private set; } = new();
    
    public string PreferredColor { get; private set; } = "Azul";
    
    //
    public ColorService(IJSRuntime jsRuntime, HttpClient httpClient)
    {
        _jsRuntime = jsRuntime;
         _httpClient = httpClient;
    }
    
    public async Task InitializeAsync()
    {
      try
      {
          Colors = await _httpClient.GetFromJsonAsync<List<ColorModel>>("api/generics/colors");

          Colors ??= new List<ColorModel>();

          string storedColor = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", LocalStorageKey);

          if (!string.IsNullOrWhiteSpace(storedColor) && Colors.Any(c => c.Name == storedColor))
          {
              PreferredColor = storedColor;
          }
          else if (Colors.Any())
          {
              PreferredColor = Colors.First().Name;
          }
      }
      catch (Exception ex)
      {
          await _jsRuntime.InvokeVoidAsync("console.error", $"Error cargando colores: {ex.Message}");
          // Colors = new List<ColorModel>();
          if (Colors == null || !Colors.Any())
              Colors = new List<ColorModel>
              {
                 new ColorModel() {
                      Id = Generate(Alphabets.LowercaseLettersAndDigits, 10),
                      Name = "Azul",
                      Primary = "from-blue-500 to-purple-600",
                      Secondary = "from-blue-600 to-purple-700",
                      Gradient = "bg-gradient-to-r from-blue-500 to-purple-600",
                      Hover = "hover:bg-gradient-to-r hover:from-blue-600 hover:to-purple-700"
                  }
              };
          PreferredColor = "Azul";
      }
    }
    
    public async Task SetPreferredColorAsync(string colorName)
    {
        PreferredColor = colorName;
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", LocalStorageKey, colorName);
        
        // 🔔 Notificamos a todos los que escuchan
        OnColorChanged?.Invoke();
    }

    public ColorModel GetCurrentColor()
    {
        if (Colors == null || !Colors.Any())
            return new ColorModel {
                Id = Generate(Alphabets.LowercaseLettersAndDigits, 10),
                Name = "Azul",
                Primary = "from-blue-500 to-purple-600",
                Secondary = "from-blue-600 to-purple-700",
                Gradient = "bg-gradient-to-r from-blue-500 to-purple-600",
                Hover = "hover:bg-gradient-to-r hover:from-blue-600 hover:to-purple-700"
            };
        
        return Colors!.FirstOrDefault(c => c.Name == PreferredColor) ?? Colors!.First();
    }

    public string GetButtonClassess()
    {
        ColorModel? color = GetCurrentColor();

        return $"{color.Gradient} {color.Hover} text-white font-medium cursor-pointer";
    }

}