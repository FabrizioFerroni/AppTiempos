using System.Text.Json;

namespace AppTiemposV3.Web.Utils;

public static class ConsolePrintHelper
{
    public static void PrintAsJson(object data)
    {
        string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(json);
    }
}