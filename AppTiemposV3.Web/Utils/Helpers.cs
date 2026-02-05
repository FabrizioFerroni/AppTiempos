using System.Text.Json;
using static System.Text.Encodings.Web.JavaScriptEncoder;
namespace AppTiemposV3.Web.Utils;

public static class Helpers
{
    private static readonly Dictionary<string, string> TailwindColorMap = new()
    {
        // RED
        { "red-500", "#ef4444" },
        { "red-600", "#dc2626" },

        // PINK
        { "pink-600", "#db2777" },

        // BLUE
        { "blue-500", "#3b82f6" },
        { "blue-600", "#2563eb" },

        // PURPLE
        { "purple-600", "#9333ea" },
        { "purple-500", "#a855f7" },

        // GREEN
        { "green-500", "#22c55e" },
        { "green-600", "#16a34a" },

        // EMERALD
        { "emerald-600", "#059669" },

        // ORANGE
        { "orange-500", "#f97316" },
        { "orange-600", "#ea580c" },

        // INDIGO
        { "indigo-600", "#4f46e5" }
    };

    public static string Plural(int count, string singular, string plural) =>
        count == 1 ? singular : plural;
    
    public static void PrintAsJson(object data)
    {
        string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true, Encoder = UnsafeRelaxedJsonEscaping });
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

    public static string GetInitials(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        // Split the string by spaces, removing empty entries
        string[] parts = value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        // Select the first character of each valid word, convert to uppercase, and concatenate
        string? initials = string.Concat(parts
            .Where(x => !string.IsNullOrWhiteSpace(x) && char.IsLetter(x[0]))
            .Select(x => char.ToUpper(x[0])));

        return initials;
    }

    public static string GetHexColorGradient(string gradient)
    {
        if (string.IsNullOrWhiteSpace(gradient))
            return "#2196F3";

        string? fromClass = gradient
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault(x => x.StartsWith("from-"));

        if (fromClass is null)
            return "#2196F3";

        string colorKey = fromClass.Replace("from-", "");

        return TailwindColorMap.TryGetValue(colorKey, out string? hex)
            ? hex
            : "#2196F3";
    }


    public static Dictionary<ReportTable, string> BuildAliases()
    {
        Dictionary<ReportTable, string>? result = new Dictionary<ReportTable, string>();
        HashSet<string>? used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (ReportTable table in Enum.GetValues<ReportTable>())
        {
            string name = table.ToString();
            string alias = GenerateAlias(name, used);

            result[table] = alias;
            used.Add(alias);
        }

        return result;
    }

    private static string GenerateAlias(string name, HashSet<string> used)
    {
        string? camel = string.Concat(
            name.Where((c, i) => i == 0 || char.IsUpper(c))
        ).ToUpper();

        if (!used.Contains(camel))
            return camel;

        for (int len = 1; len <= name.Length; len++)
        {
            string? candidate = name.Substring(0, len).ToUpper();
            if (!used.Contains(candidate))
                return candidate;
        }

        int i = 2;
        while (used.Contains(name[0] + i.ToString()))
            i++;

        return name[0] + i.ToString();
    }
}