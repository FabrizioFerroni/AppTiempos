namespace AppTiemposV3.SharedClases.Utilidades;

public static class SanitizeText
{
    public static string SanitizeTitulo(string? titulo)
    {
        if (string.IsNullOrWhiteSpace(titulo))
            return string.Empty;

        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Concat(titulo.Where(c => !invalidChars.Contains(c)));
    }
}