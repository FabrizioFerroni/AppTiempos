using System.Text.Json;

namespace AppTiemposV3.SharedClases.Utilidades;

public class JwtHelper
{
    public static Dictionary<string, object>? DecodePayload(string token)
    {
        var parts = token.Split('.');
        if (parts.Length < 2) return null;

        var payload = parts[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var json = System.Text.Encoding.UTF8.GetString(jsonBytes);

        return JsonSerializer.Deserialize<Dictionary<string, object>>(json);
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}