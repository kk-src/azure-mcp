// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using AzureMcp.Services.Interfaces;
using Microsoft.Azure.Management.PostgreSQL.FlexibleServers;
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

    public async Task<string> ExecuteQueryAsync(string server, string user, string query)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(query, connection);
        var result = await command.ExecuteScalarAsync();

        return result?.ToString() ?? string.Empty;
    }

    public async Task<List<string>> ListPostgreSqlServersAsync(string subscriptionId, string? tenantId = null)
    {
        var credentials = new DefaultAzureCredential();
        var token = await credentials.GetTokenAsync(new TokenRequestContext(new[] { "https://management.azure.com/.default" }));
        var creds = new Microsoft.Rest.TokenCredentials(token.Token);

        using var client = new Microsoft.Azure.Management.PostgreSQL.FlexibleServers.PostgreSQLManagementClient(creds)
        {
            SubscriptionId = subscriptionId
        };

        var serverList = await client.Servers.ListAsync();
        return [.. serverList.Select(server => server.Name)];
    }
}