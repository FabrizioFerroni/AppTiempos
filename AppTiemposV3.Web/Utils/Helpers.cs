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
}