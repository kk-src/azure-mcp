// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Services.Interfaces;
using Npgsql;

namespace AzureMcp.Services.Azure.PostgreSQL;

public class PostgreSQLService : IPostgreSQLService
{
    private readonly string _connectionString;

    public PostgreSQLService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<List<string>> ListDatabasesAsync()
    {
        var databases = new List<string>();
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = "SELECT datname FROM pg_database WHERE datistemplate = false;";
        await using var command = new NpgsqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            databases.Add(reader.GetString(0));
        }

        return databases;
    }
}