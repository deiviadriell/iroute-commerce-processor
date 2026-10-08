using System.Data;
using IRoute.Commerce.Application.Abstractions;
using IRoute.Commerce.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace IRoute.Commerce.Infrastructure.Persistence;

public class UserRepository : IUserRepository
{
    private readonly string _connectionString;

    public UserRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Commerce")!;
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand("dbo.sp_get_user_by_username", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.Add(new SqlParameter("@username", SqlDbType.VarChar, 50) { Value = username });

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow);

        if (!await reader.ReadAsync())
            return null;

        return new User
        {
            Id = reader.GetInt32(0),
            Username = reader.GetString(1),
            PasswordHash = reader.GetString(2),
            FullName = reader.GetString(3),
            IsActive = reader.GetBoolean(4)
        };
    }
}
