namespace AppTiemposV3.Web.Utils;

public static class CssHelper
{

    public static string Cn(params string?[] classes)
    {
        return string.Join(" ", classes.Where(c => !string.IsNullOrWhiteSpace(c)));
    }
}