using static System.Globalization.CultureInfo;
using static System.Globalization.DateTimeStyles;

namespace AppTiemposV3.Web.Utils
{
    public class ReportJoinDto
    {
        public string Table { get; set; } = default!;
        public string Field { get; set; } = default!;
        public string TargetField { get; set; } = default!;
        public string JoinType { get; set; } = default!;
    }

    public class ReportTableMetadataDto
    {
        public string Table { get; set; } = default!;
        public List<TableField> Fields { get; set; } = new();
        public List<ReportJoinDto> Joins { get; set; } = new();
    }

    public enum ReportTable
    {
        Activities,
        Trainings,
        Rejections,
        RechazosDetails,
        Requirements,
        Users,
        Categories
    }

    public class TableField
    {
        public string Value { get; set; } = default!;
        public string Label { get; set; } = default!;
        public string Type { get; set; } = default!;
        public bool Nullable { get; set; } = false;
        public string? DefaultValue { get; set; } = default!;
    }

    public class JoinDefinition
    {
        public ReportTable Table { get; set; }
        public ReportTable? ParentTable { get; set; }
        public string Nombre { get; set; } = default!;
        public string Field { get; set; } = default!;
        public string TargetField { get; set; } = default!;
        public JoinType JoinType { get; set; }
    }

    public enum JoinType
    {
        Inner,
        Left
    }

    public record ReportTableUi(
        ReportTable Table,
        string Key,
        string Title,
        string Description
    );

    public static class ReportMetadata
    {
        public static readonly Dictionary<ReportTable, List<TableField>> TableFields =
        new()
        {
            [ReportTable.Activities] = new()
            {
                new() { Value = "Id", Label = "ID", Type = "uuid", Nullable = false},
                new() { Value = "StartDate", Label = "Fecha Inicio", Type = "date" , Nullable = false },
                new() { Value = "StartTime", Label = "Hora Inicio", Type = "time", Nullable = false },
                new() { Value = "EndTime", Label = "Hora Fin", Type = "time" , Nullable = true},
                new() { Value = "Description", Label = "Descripción", Type = "varchar(255)", Nullable = true},
                new() { Value = "IsLoaded", Label = "Fue cargado", Type = "tinyint", Nullable = false, DefaultValue = "0" },
                new() { Value = "StatusMessage", Label = "Estado", Type = "varchar(255)", Nullable = false,  DefaultValue = "in-progress" },
                new() { Value = "Comment", Label = "Comentario", Type = "varchar(255)", Nullable = true },
                new() { Value = "UserId", Label = "Usuario ID", Type = "uuid", Nullable = false },
                new() { Value = "RequirementId", Label = "Requerimiento ID", Type = "uuid", Nullable = false },
                new() { Value = "Etapa", Label = "Etapa", Type = "number", Nullable = false, DefaultValue = "1" },
            },

            [ReportTable.Trainings] = new()
            {
                new() { Value = "Id", Label = "ID", Type = "uuid", Nullable = false },
                new() { Value = "StartDate", Label = "Fecha Inicio", Type = "date", Nullable = false  },
                new() { Value = "StartTime", Label = "Hora Inicio", Type = "time", Nullable = false },
                new() { Value = "EndTime", Label = "Hora Fin", Type = "time", Nullable = true },
                new() { Value = "Capacitator", Label = "Capacitador", Type = "varchar(255)", Nullable = false },
                new() { Value = "Description", Label = "Descripción", Type = "longtext", Nullable = true },
                new() { Value = "IsLoaded", Label = "Fue cargado", Type = "tinyint", Nullable = false},
                new() { Value = "Status", Label = "Estado", Type = "longtext", Nullable = false},
                new() { Value = "Notes", Label = "Notas", Type = "longtext", Nullable = true},
                new() { Value = "UserId", Label = "Usuario ID", Type = "uuid", Nullable = false },
                new() { Value = "RequirementId", Label = "Requerimiento ID", Type = "uuid", Nullable = false },
            },

            [ReportTable.Rejections] = new()
            {
                new() { Value = "Id", Label = "ID", Type = "string", Nullable = false },
                new() { Value = "TotalRejections", Label = "Rechazos totales", Type = "int", Nullable = false },
                new() { Value = "Status", Label = "Estado", Type = "longtext", Nullable = false},
                new() { Value = "IsResolve", Label = "Resuelto", Type = "boolean", Nullable = false },
                new() { Value = "ResolvedDate", Label = "Fecha Resuelto", Type = "datetime", Nullable = true },
                new() { Value = "RequirementId", Label = "Requerimiento ID", Type = "uuid", Nullable = false },
                new() { Value = "UserId", Label = "Usuario ID", Type = "uuid", Nullable = false },
            },

            [ReportTable.RechazosDetails] = new()
            {
                new() { Value = "Id", Label = "ID", Type = "uuid", Nullable = false },
                new() { Value = "RejectionDate", Label = "Fecha Rechazo", Type = "date", Nullable = false },
                new() { Value = "RejectionReason", Label = "Motivo", Type = "varchar(255)", Nullable = false },
                new() { Value = "RejectionDetails", Label = "Detalles", Type = "varchar(255)", Nullable = false },
                new() { Value = "SolutionDate", Label = "Fecha Solución", Type = "date", Nullable = true },
                new() { Value = "SolutionDetails", Label = "Detalles Solución", Type = "varchar(255)", Nullable = true },
                new() { Value = "EstimatedFixTime", Label = "Tiempo Estimado", Type = "time", Nullable = true },
                new() { Value = "ActualFixTime", Label = "Tiempo Real", Type = "time", Nullable = true },
                new() { Value = "Status", Label = "Estado", Type = "longtext", Nullable = false },
                new() { Value = "RejectionId", Label = "Rechazo ID", Type = "uuid" , Nullable = false},
                new() { Value = "UserId", Label = "Usuario ID", Type = "uuid", Nullable = false },
            },

            [ReportTable.Requirements] = new()
            {
                new() { Value = "Id", Label = "ID", Type = "uuid", Nullable = false },
                new() { Value = "ReqID", Label = "REQ ID", Type = "varchar(15)", Nullable = false },
                new() { Value = "Titulo", Label = "Título", Type = "longtext", Nullable = false },
                new() { Value = "Cliente", Label = "Cliente", Type = "longtext", Nullable = false },
                new() { Value = "StoryPoint", Label = "Puntos de función", Type = "longtext", Nullable = false },
                new() { Value = "Descripcion", Label = "Descripción", Type = "longtext", Nullable = true },
                new() { Value = "ConjuntoCambios", Label = "Conjuntos de cambios", Type = "json", Nullable = true },
                new() { Value = "FolderId", Label = "ID de carpeta", Type = "int", Nullable = true },
                new() { Value = "EtapaActual", Label = "Etapa actual", Type = "int", Nullable = false },
                new() { Value = "Estado", Label = "Estado", Type = "int", Nullable = false },
                new() { Value = "CategoryId", Label = "Categoría ID", Type = "uuid", Nullable = false },
                new() { Value = "UserId", Label = "Usuario ID", Type = "uuid", Nullable = false },
            },

            [ReportTable.Users] = new()
            {
                new() { Value = "Id", Label = "ID", Type = "uuid", Nullable = false  },
                new() { Value = "FullName", Label = "Nombre Completo", Type = "varchar(150)", Nullable = false  },
                new() { Value = "Email", Label = "Email", Type = "varchar(256)", Nullable = true  },
                new() { Value = "Area", Label = "Área", Type = "int", Nullable = false  },
                new() { Value = "UserName", Label = "Nombre de Usuario", Type = "varchar(256)", Nullable = true },
            },

            [ReportTable.Categories] = new()
            {
                new() { Value = "Id", Label = "ID", Type = "uuid", Nullable = false },
                new() { Value = "Name", Label = "Nombre", Type = "varchar(255)", Nullable = false },
                new() { Value = "Description", Label = "Descripción", Type = "longtext", Nullable = true },
                new() { Value = "Color", Label = "Color", Type = "longtext", Nullable = true },
                new() { Value = "Slug", Label = "Slug", Type = "longtext", Nullable = true },
            }
        };

        public static readonly Dictionary<ReportTable, List<JoinDefinition>> AvailableJoins =
            new()
            {
                [ReportTable.Activities] = new()
                {
                new() { Table = ReportTable.Users, Nombre = "Usuarios", Field = "UserId", TargetField = "Id", JoinType = JoinType.Inner },
                new() { Table = ReportTable.Requirements, Nombre = "Requeriments",  Field = "RequerimentId", TargetField = "Id", JoinType = JoinType.Inner },
                },

                [ReportTable.Trainings] = new()
                {
                new() { Table = ReportTable.Users, Nombre = "Usuarios", Field = "UserId", TargetField = "Id", JoinType = JoinType.Inner  },
                new() { Table = ReportTable.Requirements, Nombre = "Requeriments", Field = "RequerimentId", TargetField = "Id", JoinType = JoinType.Inner  },
                },

                [ReportTable.Rejections] = new()
                {
                new() { Table = ReportTable.Users, Nombre = "Usuarios", Field = "UserId", TargetField = "Id", JoinType = JoinType.Inner  },
                new() { Table = ReportTable.Requirements, Nombre = "Requeriments", Field = "RequerimentId", TargetField = "Id", JoinType = JoinType.Inner  },
                },

                [ReportTable.RechazosDetails] = new()
                {
                new() { Table = ReportTable.Users, Nombre = "Usuarios", Field = "UserId", TargetField = "Id", JoinType = JoinType.Inner  },
                new() { Table = ReportTable.Rejections, Nombre = "Rechazos", Field = "RejectionId", TargetField = "Id" , JoinType = JoinType.Inner},
                },

                [ReportTable.Requirements] = new()
                {
                new() { Table = ReportTable.Users, Nombre = "Usuarios", Field = "UserId", TargetField = "Id", JoinType = JoinType.Inner  },
                new() { Table = ReportTable.Categories, Nombre = "Categories", Field = "CategoryId", TargetField = "Id", JoinType = JoinType.Inner  },
                },
            };

        public static bool IsJoinAllowed(ReportTable baseTable, ReportTable targetTable)
        {
            return AvailableJoins.TryGetValue(baseTable, out List<JoinDefinition>? joins)
                   && joins.Any(j => j.Table == targetTable);
        }

        public static IReadOnlyList<JoinDefinition> GetAllowedJoins(ReportTable baseTable)
        {
            return AvailableJoins.TryGetValue(baseTable, out List<JoinDefinition>? joins)
                ? joins
                : Array.Empty<JoinDefinition>();
        }

        public static ReportTableMetadataDto BuildMetadata(ReportTable table)
        {
            return new ReportTableMetadataDto
            {
                Table = table.ToString(),
                Fields = ReportMetadata.TableFields[table],
                Joins = ReportMetadata.AvailableJoins[table]
                    .Select(j => new ReportJoinDto
                    {
                        Table = j.Table.ToString(),
                        Field = j.Field,
                        TargetField = j.TargetField,
                        JoinType = j.JoinType.ToString().ToUpper()
                    })
                    .ToList()
            };
        }

        public static readonly List<ReportTableUi> TablesUi = new()
        {
            new(ReportTable.Activities, "activities", "Actividades", "Actividades registradas"),
            new(ReportTable.Trainings, "trainings", "Capacitaciones", "Capacitaciones"),
            new(ReportTable.Rejections, "rechazos", "Rechazos", "Rechazos"),
            new(ReportTable.RechazosDetails, "rechazos_detalles", "Rechazos Detalles", "Detalle de rechazos"),
            new(ReportTable.Requirements, "requirements", "Requerimientos", "Requerimientos"),
            new(ReportTable.Users, "usuarios", "Usuarios", "Usuarios"),
            new(ReportTable.Categories, "categories", "Categorías", "Categorías"),
        };

        public static IReadOnlyList<TableField> GetFields(ReportTable table)
        {
            return TableFields.TryGetValue(table, out List<TableField>? fields)
                ? fields
                : Array.Empty<TableField>();
        }

        public static IReadOnlyList<JoinDefinition> GetJoins(ReportTable table)
        {
            return AvailableJoins.TryGetValue(table, out List<JoinDefinition>? fields)
                ? fields
                : Array.Empty<JoinDefinition>();
        }

        public static bool HaveJoin(ReportTable table)
        {
            return AvailableJoins.ContainsKey(table);
        }

        public static string NameToSpanish(string Nombre)
        {
            string tableNameSpanish = string.Empty;

            if(Nombre == "Requeriments")
            {
                tableNameSpanish = "Requerimientos";
            }else if(Nombre == "Requirements")
            {
                tableNameSpanish = "Requerimientos";
            }else if (Nombre == "Users")
            {
                tableNameSpanish = "Usuarios";
            }
            else if (Nombre == "Categories")
            {
                tableNameSpanish = "Categorías";
            }
            else if (Nombre == "Rejections")
            {
                tableNameSpanish = "Rechazos";
            }
            else
            {
                tableNameSpanish = Nombre;
            }

            return tableNameSpanish;
        }

        public static string SpanishToName(string Nombre)
        {
            string tableNameSpanish = string.Empty;

            if (Nombre == "Requerimientos")
            {
                tableNameSpanish = "Requirements";
            }
            else if (Nombre == "Usuarios")
            {
                tableNameSpanish = "Users";
            }
            else
            {
                tableNameSpanish = Nombre;
            }

            return tableNameSpanish;
        }

        public static string GetSqlOperator(string operador)
        {
            return operador switch
            {
                "Igual a" => "=",
                "Diferente de" => "<>",
                "Contiene" => "LIKE",
                "Mayor que" => ">",
                "Mayor igual que" => ">=",
                "Menor que" => "<",
                "Menor igual que" => "<=",
                "Entre" => "BETWEEN",
                "En lista" => "IN",
                "No en la lista" => "NOT IN",
                _ => "="
            };
        }

        public static string GetSqlAggregation(string agregacion, string campo)
        {
            return agregacion switch
            {
                "Suma" => $"SUM({campo})",
                "Contar" => $"COUNT({campo})",
                "Promedio" => $"AVG({campo})",
                "Minimo" => $"MIN({campo})",
                "Maximo" => $"MAX({campo})",
                "Unicos" => $"COUNT(DISTINCT {campo})",
                _ => campo
            };
        }

        public static string GetSqlAggregation(string agregacion)
        {
            return agregacion switch
            {
                "Suma" => $"SUM",
                "Contar" => $"COUNT",
                "Promedio" => $"AVG",
                "Minimo" => $"MIN",
                "Maximo" => $"MAX",
                "Unicos" => $"COUNT_DISTINCT",
                _ => ""
            };
        }

        public static string FormatFilterValue(string operador, string valor, string columnType)
        {
            if (string.IsNullOrWhiteSpace(valor)) return "''";
            valor = valor.Trim();

            if (operador == "Contiene") return $"'%{valor}%'";

            // Si el tipo en la metadata es numérico, no ponemos comillas
            if (columnType == "int" || columnType == "number" || columnType == "tinyint")
            {
                return valor;
            }

            // Para UUID, Date, Time y Varchar, usamos comillas
            return $"'{valor}'";
        }

        public static string NormalizeDate(string value)
        {
            // Formatos que quieres soportar
            string[] formats = { "dd-MM-yyyy", "dd/MM/yyyy", "yyyy-MM-dd", "yyyy/MM/dd" };

            if (DateTime.TryParseExact(value, formats, InvariantCulture, None, out DateTime date))
            {
                return date.ToString("yyyy-MM-dd");
            }

            return value; // Si no puede parsear, devuelve el original (quizás es un número)
        }

        public static string NormalizeTableName(string tableName)
        {
            return tableName switch
            {
                "Actividades" => "activities",
                "Capacitaciones" => "trainings",
                "Rechazos" => "rechazos",
                "Rechazos Detalles" => "rechazos_detalles",
                "Requerimientos" => "requirements",
                "Usuarios" => "usuarios",
                "Categorías" => "categories",
                _ => ""
            };
        }
    }


}
