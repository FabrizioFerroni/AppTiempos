using System.Text;
using System.Text.Json;
using AppTiemposV3.SharedClases.Exceptions;

namespace AppTiemposV3.SharedClases.Utilidades;

public class TokenHelper
{
    public static string CrearToken(object data)
    {
        string json = JsonSerializer.Serialize(data);
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        return Base64UrlEncode(bytes);
    }

    public static T? LeerToken<T>(string token)
    {
        try
        {
            byte[] bytes = Base64UrlDecode(token);

            string json = Encoding.UTF8.GetString(bytes);

            return JsonSerializer.Deserialize<T>(json);
        }
        catch
        {
            return default;
        }
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }

    private static byte[] Base64UrlDecode(string input)
    {
        string padded = input
            .Replace("-", "+")
            .Replace("_", "/");
        switch (padded.Length % 4)
        {
            case 2: padded += "=="; break;
            case 3: padded += "="; break;
        }
        return Convert.FromBase64String(padded);
    }
}