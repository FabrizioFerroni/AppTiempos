using System.Text.Json;

namespace AppTiemposV3.Web.Utils;

public static class Helpers
{
    public static string Plural(int count, string singular, string plural) =>
        count == 1 ? singular : plural;
    
    public static void PrintAsJson(object data)
    {
        string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(json);
    }
    
    public static async Task SafeRunAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error en fire & forget: {ex}");
        }
    }
    
    public static string FormatTwoDigits(int? num)
    {
        if (num is null) return string.Empty;
        return num.Value.ToString("D2");
    }
}