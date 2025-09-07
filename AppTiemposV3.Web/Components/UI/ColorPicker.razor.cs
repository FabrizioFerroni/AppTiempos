using AppTiemposV3.SharedClases.GenericModels;
using Microsoft.AspNetCore.Components;
using static NanoidDotNet.Nanoid;

namespace AppTiemposV3.Web.Components.UI;

public partial class ColorPicker : ComponentBase
{
    public string ColorPrefer { get; set; } = "Rojo";

    private List<ColorModel> Colors = new()
    {
        new ColorModel
        {
            Id = Generate(Alphabets.LowercaseLettersAndDigits, 10),
            Name = "Azul",
            Primary = "from-blue-500 to-purple-600",
            Secondary = "from-blue-600 to-purple-700",
            Gradient = "bg-gradient-to-r from-blue-500 to-purple-600",
            Hover = "hover:from-blue-600 hover:to-purple-700"
        },
        new ColorModel
        {
            Id = Generate(Alphabets.LowercaseLettersAndDigits, 10),
            Name = "Rojo",
            Primary = "from-red-500 to-pink-600",
            Secondary = "from-red-600 to-pink-700",
            Gradient = "bg-gradient-to-r from-red-500 to-pink-600",
            Hover = "hover:from-red-600 hover:to-pink-700"
        },
        new ColorModel
        {
            Id = Generate(Alphabets.LowercaseLettersAndDigits, 10),
            Name = "Verde",
            Primary = "from-green-500 to-emerald-600",
            Secondary = "from-green-600 to-emerald-700",
            Gradient = "bg-gradient-to-r from-green-500 to-emerald-600",
            Hover = "hover:from-green-600 hover:to-emerald-700"
        },
        new ColorModel
        {
            Id = Generate(Alphabets.LowercaseLettersAndDigits, 10),
            Name = "Naranja",
            Primary = "from-orange-500 to-red-600",
            Secondary = "from-orange-600 to-red-700",
            Gradient = "bg-gradient-to-r from-orange-500 to-red-600",
            Hover = "hover:from-orange-600 hover:to-red-700"
        },
        new ColorModel
        {
            Id = Generate(Alphabets.LowercaseLettersAndDigits, 10),
            Name = "Morado",
            Primary = "from-purple-500 to-indigo-600",
            Secondary = "from-purple-600 to-indigo-700",
            Gradient = "bg-gradient-to-r from-purple-500 to-indigo-600",
            Hover = "hover:from-purple-600 hover:to-indigo-700"
        }
    };

    private void SetColor(string name)
    {
        ColorPrefer = name;
    }
    
    private string GetButtonClass2(ColorModel color) =>
        $"w-full justify-start gap-3 {(color.Name == ColorPrefer ? color.Gradient + " text-white" : "hover:bg-gray-50 dark:hover:bg-gray-800")}";
    
    private string GetButtonClass(ColorModel color) =>
        $"rounded-md border cursor-pointer flex items-center gap-5 w-full px-4 py-2 text-sm " +
        $"dark:text-gray-300 text-gray-500 dark:border-gray-700 " +
        $"dark:focus:bg-white/5 dark:focus:text-white focus:outline-hidden " +
        $"{(color.Name == ColorPrefer 
            ? color.Gradient + " text-white" 
            : " hover:bg-gray-300 dark:hover:bg-gray-700")}";


}