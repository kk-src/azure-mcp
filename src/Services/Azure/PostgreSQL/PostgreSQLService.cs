// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using AzureMcp.Services.Interfaces;
using Microsoft.Azure.Management.PostgreSQL.FlexibleServers;
using Npgsql;
using System.Text.Json;

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

    public async Task<string> GetServerConfigAsync(string serverName, string subscriptionId, string? tenantId = null)
    {
        var credentials = new DefaultAzureCredential();
        var token = await credentials.GetTokenAsync(new TokenRequestContext(new[] { "https://management.azure.com/.default" }));
        var creds = new Microsoft.Rest.TokenCredentials(token.Token);
    
        using var client = new PostgreSQLManagementClient(creds)
        {
            SubscriptionId = subscriptionId
        };
    
        var configList = await client.Configurations.ListByServerAsync(resourceGroupName: tenantId, serverName: serverName);
        return string.Join(";", configList.Select(config => $"{config.Name}={config.Value}"));
    }

    public async Task<string> GetServerParameterAsync(string subscriptionId, string resourceGroup, string serverName, string parameterName, string? tenantId = null)
    {
        var credentials = new DefaultAzureCredential();
        var token = await credentials.GetTokenAsync(new TokenRequestContext(new[] { "https://management.azure.com/.default" }));
        var creds = new Microsoft.Rest.TokenCredentials(token.Token);

        using var client = new PostgreSQLManagementClient(creds)
        {
            SubscriptionId = subscriptionId
        };

        var parameter = await client.Configurations.GetAsync(resourceGroup, serverName, parameterName);
        return parameter?.Value ?? string.Empty;
    }

    public async Task<List<string>> ListTablesAsync(string server, string databaseName, string user)
    {
        var tables = new List<string>();
        var connectionString = $"Host={server};Database={databaseName};Username={user};";

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        var query = "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public';";
        await using var command = new NpgsqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            tables.Add(reader.GetString(0));
        }

        return tables;
    }

    public async Task<string> GetTableSchemaAsync(string server, string databaseName, string tableName, string user)
    {
        var connectionString = $"Host={server};Database={databaseName};Username={user};";

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        var query = $"SELECT column_name, data_type FROM information_schema.columns WHERE table_name = '{tableName}';";
        await using var command = new NpgsqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync();

        var schema = new List<Dictionary<string, string>>();

        while (await reader.ReadAsync())
        {
            schema.Add(new Dictionary<string, string>
            {
                { "column_name", reader.GetString(0) },
                { "data_type", reader.GetString(1) }
            });
        }

        return JsonSerializer.Serialize(schema);
    }
}