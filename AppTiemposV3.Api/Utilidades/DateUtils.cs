namespace AppTiemposV3.Api.Utilidades;

public static class DateUtils
{
    public static String ToUnixTime(DateTime dateTime)
    {
        DateTimeOffset dto = new DateTimeOffset(dateTime.ToUniversalTime());
        return dto.ToUnixTimeSeconds().ToString();
    }
}