using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Database.Seeders;

internal static class SeederSchemaGuard
{
    public static async Task<bool> HasColumnAsync(
        ApplicationDbContext db,
        string tableName,
        string columnName,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        if (!db.Database.IsRelational())
        {
            return true;
        }

        DbConnection connection = db.Database.GetDbConnection();
        bool openedHere = false;
        try
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync(cancellationToken);
                openedHere = true;
            }

            DataTable schema = connection.GetSchema("Columns");
            string normalizedTable = tableName.Trim().ToUpperInvariant();
            string normalizedColumn = columnName.Trim().ToUpperInvariant();

            foreach (DataRow row in schema.Rows)
            {
                string? table = row["TABLE_NAME"]?.ToString();
                string? column = row["COLUMN_NAME"]?.ToString();
                if (string.IsNullOrWhiteSpace(table) || string.IsNullOrWhiteSpace(column))
                {
                    continue;
                }

                if (table.Trim().ToUpperInvariant() == normalizedTable
                    && column.Trim().ToUpperInvariant() == normalizedColumn)
                {
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            // 中文註解：若無法判斷 Schema，先略過 Seeder，避免整體啟動失敗。
            logger.LogWarning(ex, "無法確認資料表欄位 {Table}.{Column} 是否存在，Seeder 將略過該欄位。",
                tableName, columnName);
            return false;
        }
        finally
        {
            if (openedHere && connection.State == ConnectionState.Open)
            {
                await connection.CloseAsync();
            }
        }

        return false;
    }
}
