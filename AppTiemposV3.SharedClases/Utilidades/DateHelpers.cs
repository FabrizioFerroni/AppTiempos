namespace AppTiemposV3.SharedClases.Utilidades;

public class DateHelpers
{
    public static string FormatTimestamp(DateTime timestamp)
    {
        DateTime now = DateTime.Now;
        TimeSpan diff = now - timestamp;

        int minutes = (int)Math.Floor(diff.TotalMinutes);
        int hours = (int)Math.Floor(diff.TotalHours);
        int days = (int)Math.Floor(diff.TotalDays);

        if (minutes < 1) return "Ahora";
        if (minutes < 60) return $"{minutes}m";
        if (hours < 24) return $"{hours}h";
        return $"{days}d";
    }

    public static string FormatDateOnly(DateTime? date)
    {
        if (date is null)
        {
            return string.Empty;
        }
        
        return date.Value.ToString("dd/MM/yyyy");
    }
    
    public static string FormatDateAndTime(DateTime? date)
    {
        if (date is null)
        {
            return string.Empty;
        }
        
        return date.Value.ToString("dd/MM/yyyy HH:mm");
    }

}