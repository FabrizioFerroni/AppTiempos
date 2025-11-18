using System.Data;
using System.Data.Common;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using static System.Data.ConnectionState;
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
}