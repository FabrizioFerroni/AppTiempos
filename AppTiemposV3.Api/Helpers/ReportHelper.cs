using AppTiemposV3.Api.Data;
using AppTiemposV3.SharedClases.DTOs.Reports;
using MySqlConnector;
using System.Text;
using static AppTiemposV3.Api.Helpers.DatabaseHelper;
using static AppTiemposV3.Api.Utilidades.Helpers;
using static System.Globalization.CultureInfo;
using static System.Globalization.NumberStyles;

namespace AppTiemposV3.Api.Helpers
{
    public static class ReportHelper
    {
        public static async Task<List<Dictionary<string, object?>>> BuildAndExecuteQueryRaw(string queryRaw, AppDbContext dbCxt)
        {
            return await EjecutarQueryDinamica(queryRaw, dbCxt);
        }

        public static async Task<List<Dictionary<string, object?>>> BuildAndExecuteQueryResult(QueryRequestDTO queryRequest, AppDbContext dbCxt)
        {
            QueryRequestDTO qDto = queryRequest;
            StringBuilder sb = new StringBuilder();
            List<MySqlParameter>? parameters = new List<MySqlParameter>();

            sb.AppendLine("SELECT");

            List<string>? columns = new List<string>();

            if (qDto.Fields?.Any() == true)
            {
                foreach (SelectedFieldDTO? field in qDto.Fields)
                {
                    string tableAlias = BuildAliases(field.Table);
                    string aliasPart = string.IsNullOrWhiteSpace(field.Alias)
                        ? ""
                        : $" AS {(field.Alias.Contains(" ") ? $"`{field.Alias}`" : field.Alias)}";

                    columns.Add($"  {tableAlias}.{field.Field}{aliasPart}");
                }
            }

            if (qDto.Metrics?.Any() == true)
            {
                foreach (MetricDTO? metric in qDto.Metrics)
                {
                    string tableAlias = BuildAliases(RenameTableName(metric.Table));
                    string aggFunc = metric.Aggregation.ToUpper() == "COUNT_DISTINCT"
                        ? "COUNT(DISTINCT "
                        : $"{metric.Aggregation.ToUpper()}(";

                    string labelPart = string.IsNullOrWhiteSpace(metric.Label)
                        ? ""
                        : $" AS {(metric.Label.Contains(" ") ? $"`{metric.Label}`" : metric.Label)}";

                    columns.Add($"  {aggFunc}{tableAlias}.{metric.Field}){labelPart}");
                }
            }

            if (columns.Count > 0)
            {
                sb.AppendLine(string.Join("," + Environment.NewLine, columns));
            }
            else
            {
                sb.AppendLine("  *");
            }

            sb.AppendLine($"FROM {RenameTableName(qDto.BaseTable)} AS {BuildAliases(qDto.BaseTable)}");

            if (qDto.Joins is not null)
            {
                List<JoinDTO> joins = qDto.Joins;

                foreach (JoinDTO join in joins)
                {
                    string tableAlias = BuildAliases(RenameTableName(join.Table));
                    string parentTableAlias = BuildAliases(RenameTableName(join.ParentTable));

                    sb.AppendLine($"{join.Type} {RenameTableName(join.Table)} AS {tableAlias} ON {tableAlias}.{join.TargetField} = {parentTableAlias}.{join.Field}");
                }
            }

            string finalQueryPart = string.Empty;

            if (qDto.Filters is not null && qDto.Filters.Any())
            {
                StringBuilder whereClause = new StringBuilder("WHERE ");
                List<FilterDTO> filters = qDto.Filters;

                for (int i = 0; i < filters.Count; i++)
                {
                    FilterDTO? filter = filters[i];
                    string tableAlias = BuildAliases(filter.Table);

                    if (i > 0)
                    {
                        whereClause.Append($"{Environment.NewLine} AND ");
                    }

                    string formattedValue = FormatSqlValue(filter.Value, filter.Operator);

                    whereClause.Append($"{tableAlias}.{filter.Field} {filter.Operator} {formattedValue}");
                }

                finalQueryPart = whereClause.ToString();
            }

            sb.AppendLine(finalQueryPart);

            if (qDto.GroupBy is not null && qDto.GroupBy.Any() && qDto.Metrics!.Any())
            {
                sb.AppendLine("GROUP BY ");

                List<GroupFieldDTO> groupFields = qDto.GroupBy;

                foreach (GroupFieldDTO? groupField in groupFields)
                {
                    string tableAlias = BuildAliases(RenameTableName(groupField.Table));
                    string columnGroup = $"  {tableAlias}.{groupField.Field}";

                    if (groupField != groupFields.Last())
                    {
                        columnGroup += ",";
                    }

                    sb.AppendLine(columnGroup);
                }
            }

            string sql = sb.ToString();
            return await QueryRawAsync(dbCxt, sql, parameters.ToArray());
        }

        private static string RenameTableName(string table)
        {
            return table switch
            {
                "requirements" => "requeriments",
                _ => table
            };
        }

        private static string FormatSqlValue(string value, string op)
        {
            if (string.IsNullOrEmpty(value)) return "NULL";

            op = op.ToUpper().Trim();

            if (op == "IN")
            {
                string cleanValue = value.Replace("(", "").Replace(")", "");

                IEnumerable<string>? elements = cleanValue.Split(',')
                    .Select(e => e.Trim())
                    .Select(e => {
                        if (e.StartsWith("'") && e.EndsWith("'")) return e;

                        if (double.TryParse(e, Any, InvariantCulture, out _))
                            return e;

                        return $"'{e}'";
                    });

                return $"({string.Join(", ", elements)})";
            }

            if (op == "BETWEEN") return value;

            if (value.StartsWith("'") && value.EndsWith("'")) return value;

            if (double.TryParse(value, Any, InvariantCulture, out _))
            {
                return value;
            }

            return $"'{value}'";
        }

    }
}
