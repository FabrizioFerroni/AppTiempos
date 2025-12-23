using System.Text.Json;
using static System.Text.Json.JsonSerializer;

namespace AppTiemposV3.Api.Helpers;

public static class Helpers
{
    public static void PrintAsJson(object data)
    {
        string json = Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(json);
    }
    
    public static string? NormalizeValue(object? value)
    {
        if (value == null)
            return null;

        if (value is string)
            return value.ToString();

        if (value is IEnumerable<string> list)
            return string.Join(", ", list);

        return Serialize(value);
    }
}