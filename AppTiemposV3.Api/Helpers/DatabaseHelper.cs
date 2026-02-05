using AppTiemposV3.Api.Data;
using AppTiemposV3.SharedClases.Exceptions;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Data;
using System.Data.Common;
using System.Security;
using System.Text.Json;
using System.Text.RegularExpressions;
using static System.Data.ConnectionState;
using static System.StringComparison;
namespace AppTiemposV3.Api.Helpers;

public static class DatabaseHelper
{
    public static async Task<List<Dictionary<string, object?>>> QueryRawAsync(
        DbContext dbContext,
        string sql,
        params MySqlParameter[] parameters)
    {
        bool shouldClose = false;
        DbConnection conn = dbContext.Database.GetDbConnection();

        if (conn.State != Open)
        {
            await conn.OpenAsync();
            shouldClose = true;
        }

        if (string.IsNullOrWhiteSpace(sql))
        {
            return null!;
        }
        
        try
        {
            await using DbCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;
            
            if (parameters is { Length: > 0 })
                cmd.Parameters.AddRange(parameters);

            List<Dictionary<string, object?>> result = new List<Dictionary<string, object?>>();
            
            await using DbDataReader? reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                Dictionary<string, object?> row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                result.Add(row);
            }
            return result;
        }
        finally
        {
            if (shouldClose)
                await conn.CloseAsync();
        }
        
    }


    /// <summary>
    /// Intenta guardar los cambios pendientes en el contexto de la base de datos.
    /// </summary>
    /// <param name="errorMessage">Mensaje de error a lanzar si no se guardan cambios.</param>
    /// <exception cref="InternalServerErrorException">
    /// Se lanza si no se guardan cambios en la base de datos.
    /// </exception>
    public static async Task EnsureSavedAsync(string errorMessage, AppDbContext dbCxt)
    {
        int result = await dbCxt.SaveChangesAsync();
        if (result <= 0)
            throw new InternalServerErrorException(errorMessage);
    }


    public static void ValidateRawSql(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql)) return;

        string[] forbiddenKeywords = { "DROP", "DELETE", "UPDATE", "INSERT", "ALTER", "TRUNCATE", "EXEC", "GRANT", "USE", "CREATE" };

        foreach (string word in forbiddenKeywords)
        {
            if (sql.Contains(word, OrdinalIgnoreCase))
            {
                throw new SecurityException($"Comando no permitido detectado: {word}");
            }
        }
    }


    public static async Task<List<Dictionary<string, object?>>> EjecutarQueryDinamica(string sqlRaw, AppDbContext dbCxt)
    {
        List<MySqlParameter>? parameters = new List<MySqlParameter>();
        int pIndex = 0;

        // Patrón para:
        // 1. Strings: 'text'
        // 2. Números: 123 o 123.45 (que no sean parte de una palabra)
        // 3. Booleanos/Nulls: true, false, null
        string pattern = @"'([^']*)'|\b(\d+(\.\d+)?)\b|\b(true|false|null)\b";

        string sqlParametrizada = Regex.Replace(sqlRaw, pattern, (match) =>
        {
            string value = match.Value;
            string paramName = $"@p{pIndex++}";

            // CASO 1: Es un string (venga con LIKE o sea un valor exacto)
            if (value.StartsWith("'"))
            {
                string cleanValue = value.Trim('\'');
                // Si el front mandó '%JUAN%', cleanValue será %JUAN%
                parameters.Add(new MySqlParameter(paramName, cleanValue));
                return paramName;
            }

            // CASO 2: Es un número
            if (decimal.TryParse(value, out decimal num))
            {
                parameters.Add(new MySqlParameter(paramName, num));
                return paramName;
            }

            // CASO 3: Nulls o Bools
            if (value.ToLower() == "null")
            {
                parameters.Add(new MySqlParameter(paramName, DBNull.Value));
                return paramName;
            }

            // Si no entra en nada (casos raros), devolvemos el valor original
            return value;
        });

        return await QueryRawAsync(dbCxt, sqlParametrizada, parameters.ToArray());
    }
}