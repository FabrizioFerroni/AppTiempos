using AppTiemposV3.Api.Migrations;
using static System.StringComparer;

namespace AppTiemposV3.Api.Utilidades
{
    public static class Helpers
    {
        public static string BuildAliases(string table)
        {
            HashSet<string>? used = new HashSet<string>(OrdinalIgnoreCase);
            string alias = GenerateAlias(table, used);
            used.Add(alias);

            return alias;
        }

        private static string GenerateAlias(string name, HashSet<string> used)
        {
            string? camel = string.Concat(
                name.Where((c, i) => i == 0 || char.IsUpper(c))
            ).ToUpper();

            if (!used.Contains(camel))
                return camel;

            for (int len = 1; len <= name.Length; len++)
            {
                string? candidate = name.Substring(0, len).ToUpper();
                if (!used.Contains(candidate))
                    return candidate;
            }

            int i = 2;
            while (used.Contains(name[0] + i.ToString()))
                i++;

            return name[0] + i.ToString();
        }
    }
}
