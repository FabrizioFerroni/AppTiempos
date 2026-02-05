namespace AppTiemposV3.Web.Utils
{
    public static class FormatString
    {
        public static string FormatValue(object? value, string columnName)
        {
            if (value == null) return "-";

            // Manejo especial para fechas si vienen como string ISO
            if (columnName.ToLower().Contains("fecha") && DateTime.TryParse(value.ToString(), out DateTime date))
            {
                return date.ToShortDateString();
            }

            // Manejo de booleanos (true/false)
            if (value is bool b)
            {
                return b ? "Sí" : "No";
            }

            return value.ToString() ?? "-";
        }
    }
}
