namespace AppTiemposV3.SharedClases.Helpers
{
    public static class TimeFormatHelper
    {
        /// <summary>
        /// Formato con 2 dígitos después de la coma = minutos exactos, sin redondeo ni pérdida de precisión.
        /// Ej: 100 min (1h 40m) -> "1,40h" | 210 min (3h 30m) -> "3,30h" | 380 min (6h 20m) -> "6,20h"
        /// </summary>
        public static string Formato(int minutos)
        {
            minutos = Math.Max(minutos, 0);
            int horas = minutos / 60;
            int mins = minutos % 60;
            return $"{horas},{mins:D2}h";
        }

        public static double FormatoDbl(int minutos)
        {
            minutos = Math.Max(minutos, 0);
            int horas = minutos / 60;
            int mins = minutos % 60;
            return horas + (mins / 100.0);
        }

        // Si en algún DTO necesitás el formato con palabras (ej. "6h 20min")
        public static string FormatoTextoPalabras(int minutos)
        {
            minutos = Math.Max(minutos, 0);
            int horas = minutos / 60;
            int mins = minutos % 60;
            return $"{horas}h {mins}min";
        }
    }
}
