using static System.Linq.Enumerable;

namespace AppTiemposV3.Api.Utilidades;

public static class IdGenerator
{
    private static readonly char[] chars =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
    private static readonly Random random = new();

    public static string Generate(int length = 10)
    {
        return new string(Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray()).ToLower();
    }
}