using Dapper;
using KRTBankAccount.Domain.Entities;
using KRTBankAccount.Infrastructure.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace KRTBankAccount.Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public AccountRepository(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionString = _configuration.GetConnectionString("DefaultConnection");
    }

    private IDbConnection Connection => new SqlConnection(_connectionString);

    public async Task AddAsync(Account account)
    {
        const string sql = @"
            INSERT INTO Accounts (Id, HolderName, CPF, IsActive)
            VALUES (@Id, @HolderName, @CPF, @IsActive);";

        using var conn = Connection;
        await conn.ExecuteAsync(sql, new
        {
            account.Id,
            account.HolderName,
            account.CPF,
            account.IsActive
        });
    }

    public async Task<Account?> GetByIdAsync(Guid id)
    {
        const string sql = "SELECT Id, HolderName, CPF, IsActive FROM Accounts WHERE Id = @Id";

        using var conn = Connection;
        return await conn.QueryFirstOrDefaultAsync<Account>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Account>> GetAllAsync(bool? isActive = null)
    {
        using var conn = Connection;

        var sql = "SELECT Id, HolderName, CPF, IsActive FROM Accounts";

        if (isActive.HasValue)
            sql += " WHERE IsActive = @IsActive";

        var accounts = await conn.QueryAsync<Account>(sql, new { IsActive = isActive });
        return accounts;
    }

    public async Task UpdateAsync(Account account)
    {
        const string sql = @"
            UPDATE Accounts
            SET HolderName = @HolderName,
                CPF = @CPF,
                IsActive = @IsActive
            WHERE Id = @Id";

        using var conn = Connection;
        await conn.ExecuteAsync(sql, new
        {
            account.Id,
            account.HolderName,
            account.CPF,
            account.IsActive
        });
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var conn = Connection;
        var sql = "DELETE FROM Accounts WHERE Id = @Id";
        var rows = await conn.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }
}