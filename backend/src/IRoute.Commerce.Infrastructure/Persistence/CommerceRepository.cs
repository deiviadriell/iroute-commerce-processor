using System.Data;
using IRoute.Commerce.Application.Abstractions;
using IRoute.Commerce.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.Server;
using Microsoft.Extensions.Configuration;
using CommerceEntity = IRoute.Commerce.Domain.Entities.Commerce;

namespace IRoute.Commerce.Infrastructure.Persistence;

public class CommerceRepository : ICommerceRepository
{
    private static readonly SqlMetaData[] CommerceTypeColumns =
    [
        new("pc_processdate", SqlDbType.Date),
        new("pc_codcomred", SqlDbType.VarChar, 30),
        new("pc_nomcomred", SqlDbType.NVarChar, 150),
        new("pc_tipdoc", SqlDbType.VarChar, 10),
        new("pc_numdoc", SqlDbType.VarChar, 30),
        new("pc_ciudad", SqlDbType.NVarChar, 80),
        new("pc_direccion", SqlDbType.NVarChar, 250),
        new("pc_telefono", SqlDbType.VarChar, 30),
        new("pc_email", SqlDbType.VarChar, 120)
    ];

    private readonly string _connectionString;

    public CommerceRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Commerce")!;
    }

    public async Task<int> AddRangeAsync(IReadOnlyList<CommerceEntity> commerces)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = StoredProcedure(connection, "dbo.sp_create_commerce");
        command.Parameters.Add(new SqlParameter("@commerces", SqlDbType.Structured)
        {
            TypeName = "dbo.commerce_type",
            Value = ToRecords(commerces)
        });

        await connection.OpenAsync();
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    public async Task<int> ProcessAsync(DateOnly processDate)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = StoredProcedure(connection, "dbo.sp_process_commerce");
        command.Parameters.Add(new SqlParameter("@processdate", SqlDbType.Date) { Value = processDate.ToDateTime(TimeOnly.MinValue) });

        await connection.OpenAsync();
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    public async Task<IReadOnlyList<QuarantinedCommerce>> GetQuarantinedAsync(DateOnly? processDate)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = StoredProcedure(connection, "dbo.sp_list_commerce_quarantine");
        command.Parameters.Add(new SqlParameter("@processdate", SqlDbType.Date)
        {
            Value = processDate is { } date ? date.ToDateTime(TimeOnly.MinValue) : DBNull.Value
        });

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        var result = new List<QuarantinedCommerce>();
        while (await reader.ReadAsync())
        {
            result.Add(new QuarantinedCommerce
            {
                Id = reader.GetInt64(0),
                CommerceId = reader.GetInt64(1),
                ProcessDate = DateOnly.FromDateTime(reader.GetDateTime(2)),
                CodComRed = GetString(reader, 3),
                NomComRed = GetString(reader, 4),
                TipDoc = GetString(reader, 5),
                NumDoc = GetString(reader, 6),
                Ciudad = GetString(reader, 7),
                Direccion = GetString(reader, 8),
                Telefono = GetString(reader, 9),
                Email = GetString(reader, 10),
                Motivo = reader.GetString(11),
                QuarantinedAt = reader.GetDateTime(12)
            });
        }

        return result;
    }

    public async Task<IReadOnlyList<ProcessDateSummary>> GetProcessDatesAsync()
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = StoredProcedure(connection, "dbo.sp_list_commerce_process_dates");

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        var result = new List<ProcessDateSummary>();
        while (await reader.ReadAsync())
        {
            result.Add(new ProcessDateSummary
            {
                ProcessDate = DateOnly.FromDateTime(reader.GetDateTime(0)),
                Total = reader.GetInt64(1)
            });
        }

        return result;
    }

    private static SqlCommand StoredProcedure(SqlConnection connection, string name) =>
        new(name, connection) { CommandType = CommandType.StoredProcedure, CommandTimeout = 120 };

    private static string? GetString(SqlDataReader reader, int ordinal) =>
        reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);

    private static IEnumerable<SqlDataRecord> ToRecords(IEnumerable<CommerceEntity> commerces)
    {
        var record = new SqlDataRecord(CommerceTypeColumns);
        foreach (var c in commerces)
        {
            record.SetDateTime(0, c.ProcessDate.ToDateTime(TimeOnly.MinValue));
            SetString(record, 1, c.CodComRed);
            SetString(record, 2, c.NomComRed);
            SetString(record, 3, c.TipDoc);
            SetString(record, 4, c.NumDoc);
            SetString(record, 5, c.Ciudad);
            SetString(record, 6, c.Direccion);
            SetString(record, 7, c.Telefono);
            SetString(record, 8, c.Email);
            yield return record;
        }
    }

    private static void SetString(SqlDataRecord record, int ordinal, string? value)
    {
        if (value is null)
            record.SetDBNull(ordinal);
        else
            record.SetString(ordinal, value);
    }
}
