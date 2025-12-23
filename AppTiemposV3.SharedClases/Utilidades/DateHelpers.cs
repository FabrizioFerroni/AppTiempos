using System.Globalization;

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
    
    public static string FormatDateOnly(DateOnly? date)
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
    
    public static string FormatTimeTS(TimeSpan? ts)
    {
        if (ts is null)
            return string.Empty;

        return $"{(int)ts.Value.TotalHours:00}:{ts.Value.Minutes:00}";
    }

    public static string FormatDate(DateTime createdAt)
    {
        DateTime now = DateTime.Now;
        TimeSpan diff = now - createdAt;

        if (diff.TotalMinutes < 1)
            return "Hace un momento";

        if (diff.TotalMinutes < 60)
        {
            int mins = (int)Math.Floor(diff.TotalMinutes);
            return $"Hace {mins} minuto{(mins != 1 ? "s" : "")}";
        }

        if (diff.TotalHours < 24)
        {
            int hours = (int)Math.Floor(diff.TotalHours);
            return $"Hace {hours} hora{(hours != 1 ? "s" : "")}";
        }

        if (diff.TotalDays < 7)
        {
            int days = (int)Math.Floor(diff.TotalDays);
            return $"Hace {days} día{(days != 1 ? "s" : "")}";
        }

        CultureInfo? culture = new CultureInfo("es-ES");

        return createdAt.Year != now.Year
            ? createdAt.ToString("d MMM yyyy", culture)
            : createdAt.ToString("d MMM", culture);
    }

    public static string FormatDateAuditProfile(DateTime? date)
    {
        if (date is null)
        {
            return string.Empty;
        }
        CultureInfo? culture = new CultureInfo("es-ES");

        return date.Value.ToString("dd-MM-yyyy 'a las' hh:mm", culture);
        //2025 - 01 - 10 a las 15:22

    }

}