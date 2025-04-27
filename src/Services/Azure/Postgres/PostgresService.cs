// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Identity;
using AzureMcp.Services.Interfaces;
using Microsoft.Azure.Management.PostgreSQL.FlexibleServers;
using Npgsql;

namespace AzureMcp.Services.Azure.Postgres;

public class PostgresService : IPostgresService
{
    public async Task<List<string>> ListDatabasesAsync(string subscriptionId, string resourceGroup, string server, string user)
    {
        var tokenCredential = new DefaultAzureCredential();
        var accessToken = await tokenCredential.GetTokenAsync(new TokenRequestContext(["https://ossrdbms-aad.database.windows.net/.default"]));

        var creds = new Microsoft.Rest.TokenCredentials(accessToken.Token);

        using var client = new PostgreSQLManagementClient(creds)
        {
            SubscriptionId = subscriptionId
        };

        var databaseList = await client.Databases.ListByServerAsync(resourceGroup, server);
        return [.. databaseList.Select(db => db.Name)];
    }

    public async Task<string> ExecuteQueryAsync(string subscriptionId, string resourceGroup, string server, string user, string database, string query)
    {
        var tokenCredential = new DefaultAzureCredential();
        var accessToken = await tokenCredential.GetTokenAsync(new TokenRequestContext(["https://ossrdbms-aad.database.windows.net/.default"]));

        var connectionString = $"Host={server};Database={database};Username={user};Password={accessToken.Token};";
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(query, connection);
        var result = await command.ExecuteScalarAsync();
        return result?.ToString() ?? string.Empty;
    }

    public async Task<List<string>> ListTablesAsync(string subscriptionId, string resourceGroup, string server, string user, string database)
    {
        var tokenCredential = new DefaultAzureCredential();
        var accessToken = await tokenCredential.GetTokenAsync(new TokenRequestContext(["https://ossrdbms-aad.database.windows.net/.default"]));
        
        var connectionString = $"Host={server};Database={database};Username={user};Password={accessToken.Token};";
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        var query = "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public';";
        await using var command = new NpgsqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync();
        var tables = new List<string>();
        while (await reader.ReadAsync())
        {
            tables.Add(reader.GetString(0));
        }
        return tables;
    }

    public async Task<List<string>> GetTableSchemaAsync(string subscriptionId, string resourceGroup, string server, string user, string database, string table)  
    {

        var tokenCredential = new DefaultAzureCredential();
        var accessToken = await tokenCredential.GetTokenAsync(new TokenRequestContext(["https://ossrdbms-aad.database.windows.net/.default"]));       
        var connectionString = $"Host={server};Database={database};Username={user};Password={accessToken.Token};";

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        var query = $"SELECT column_name, data_type FROM information_schema.columns WHERE table_name = '{table}';";
        await using var command = new NpgsqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync();
        var schema = new List<string>();
        while (await reader.ReadAsync())
        {
            schema.Add($"{reader.GetString(0)}: {reader.GetString(1)}");
        }
        return schema;
    }

     public async Task<string> GetServerConfigAsync(string subscriptionId, string resourceGroup, string user, string server)
    {
        var tokenCredential = new DefaultAzureCredential();
        var accessToken = await tokenCredential.GetTokenAsync(new TokenRequestContext(["https://ossrdbms-aad.database.windows.net/.default"]));
        var creds = new Microsoft.Rest.TokenCredentials(accessToken.Token);

        using var client = new PostgreSQLManagementClient(creds)
        {
            SubscriptionId = subscriptionId
        };
    
        var configList = await client.Configurations.ListByServerAsync(resourceGroupName: resourceGroup, serverName: server);
        return string.Join(";", configList.Select(config => $"{config.Name}={config.Value}"));
    }

    public async Task<string> GetServerParameterAsync(string subscriptionId, string resourceGroup, string user, string server, string param)
    {
        var tokenCredential = new DefaultAzureCredential();
        var accessToken = await tokenCredential.GetTokenAsync(new TokenRequestContext(["https://ossrdbms-aad.database.windows.net/.default"]));

        var creds = new Microsoft.Rest.TokenCredentials(accessToken.Token);

        using var client = new PostgreSQLManagementClient(creds)
        {
            SubscriptionId = subscriptionId
        };
        var configList = await client.Configurations.ListByServerAsync(resourceGroupName: resourceGroup, serverName: server);
        var config = configList.FirstOrDefault(c => c.Name.Equals(param, StringComparison.OrdinalIgnoreCase));
        
        if (config != null)
        {
            return config.Value;
        }
        else
        {
            throw new ArgumentException($"Parameter '{param}' not found.");
        }                
    }

    public async Task<List<string>> ListServersAsync(string subscriptionId, string resourceGroup, string user)
    {
        var tokenCredential = new DefaultAzureCredential();
        var accessToken = await tokenCredential.GetTokenAsync(new TokenRequestContext(["https://ossrdbms-aad.database.windows.net/.default"]));

        var creds = new Microsoft.Rest.TokenCredentials(accessToken.Token);

        using var client = new PostgreSQLManagementClient(creds)
        {
            SubscriptionId = subscriptionId
        };

        var serverList = await client.Servers.ListByResourceGroupAsync(resourceGroup);
        return [.. serverList.Select(server => server.Name)];
    }   
}