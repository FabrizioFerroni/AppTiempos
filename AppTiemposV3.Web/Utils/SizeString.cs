using static System.Math;
using static System.String;

namespace AppTiemposV3.Web.Utils
{
    public static class SizeString
    {
        public static String FormatearTamaño(long bytes)
        {
            if (bytes <= 0) return "0 B";
            if (bytes < 1024) return bytes + " B";

            string[] unidades = { "B", "KB", "MB", "GB", "TB", "PB" };

            int exponente = (int)(Log(bytes) / Log(1024));
            string unidad = unidades[exponente];

            double resultado = bytes / Pow(1024, exponente);

            return Format("{0:0.0} {1}", resultado, unidad);
        }
    }
}
