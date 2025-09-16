using System.Text.Json;
using AppTiemposV3.SharedClases.GenericModels;
using static NanoidDotNet.Nanoid;

string? outputPath = args.Length > 0 ? args[0] : "colors.json";

List<ColorModel> colors =
       [
           new()
           {
               Id = Generate(Alphabets.LowercaseLettersAndDigits, 10),
               Name = "Azul",
               Primary = "from-blue-500 to-purple-600",
               Secondary = "from-blue-600 to-purple-700",
               Gradient = "bg-gradient-to-r from-blue-500 to-purple-600",
               Hover = "hover:bg-gradient-to-r hover:from-blue-600 hover:to-purple-700"
           },

           new()
           {
               Id = Generate(Alphabets.LowercaseLettersAndDigits, 10),
               Name = "Rojo",
               Primary = "from-red-500 to-pink-600",
               Secondary = "from-red-600 to-pink-700",
               Gradient = "bg-gradient-to-r from-red-500 to-pink-600",
               Hover = "hover:bg-gradient-to-r hover:from-red-600 hover:to-pink-700"
           },

           new()
           {
               Id = Generate(Alphabets.LowercaseLettersAndDigits, 10),
               Name = "Verde",
               Primary = "from-green-500 to-emerald-600",
               Secondary = "from-green-600 to-emerald-700",
               Gradient = "bg-gradient-to-r from-green-500 to-emerald-600",
               Hover = "hover:bg-gradient-to-r hover:from-green-600 hover:to-emerald-700"
           },

           new()
           {
               Id = Generate(Alphabets.LowercaseLettersAndDigits, 10),
               Name = "Naranja",
               Primary = "from-orange-500 to-red-600",
               Secondary = "from-orange-600 to-red-700",
               Gradient = "bg-gradient-to-r from-orange-500 to-red-600",
               Hover = "hover:bg-gradient-to-r hover:from-orange-600 hover:to-red-700"
           },

           new()
           {
               Id = Generate(Alphabets.LowercaseLettersAndDigits, 10),
               Name = "Morado",
               Primary = "from-purple-500 to-indigo-600",
               Secondary = "from-purple-600 to-indigo-700",
               Gradient = "bg-gradient-to-r from-purple-500 to-indigo-600",
               Hover = "hover:bg-gradient-to-r hover:from-purple-600 hover:to-indigo-700"
           }
       ];
       
string? json = JsonSerializer.Serialize(colors, new JsonSerializerOptions { WriteIndented = true });

Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
File.WriteAllText(outputPath, json);

Console.WriteLine($"✅ Colors.json generado en {outputPath}");