// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.PostgreSql.FlexibleServers;
using Azure.ResourceManager.Resources;
using AzureMcp.Services.Interfaces;
using Npgsql;
using System.Text;
using System.Text.Json;

namespace AzureMcp.Services.Azure.Postgres;

public class PostgresService : IPostgresService
{
    public async Task<List<string>> ListDatabasesAsync(string subscriptionId, string resourceGroup, string server, string user)
    {
        var tokenCredential = new DefaultAzureCredential();
        var accessToken = await tokenCredential.GetTokenAsync(new TokenRequestContext(["https://ossrdbms-aad.database.windows.net/.default"]));

        var connectionString = $"Host={server};Database=postgres;Username={user};Password={accessToken.Token};";

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        var query = "SELECT datname FROM pg_database WHERE datistemplate = false;";
        await using var command = new NpgsqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync();
        var dbs = new List<string>();
        while (await reader.ReadAsync())
        {
            dbs.Add(reader.GetString(0));
        }
        return dbs;
    }

    public async Task<string> ExecuteQueryAsync(string subscriptionId, string resourceGroup, string server, string user, string database, string query)
    {
        var tokenCredential = new DefaultAzureCredential();
        var accessToken = await tokenCredential.GetTokenAsync(new TokenRequestContext(["https://ossrdbms-aad.database.windows.net/.default"]));

        var connectionString = $"Host={server};Database={database};Username={user};Password={accessToken.Token};";
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync();

        var columnNames = Enumerable.Range(0, reader.FieldCount)
                               .Select(reader.GetName)
                               .ToArray();
        var rows = new List<object?[]>();
        while (await reader.ReadAsync())
        {
            var row = Enumerable.Range(0, reader.FieldCount)
                                .Select(i => reader.IsDBNull(i) ? null : reader.GetValue(i))
                                .ToArray();
            rows.Add(row);
        }

        return JsonSerializer.Serialize(new { columnNames, rows });
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
        var credential = new DefaultAzureCredential();
        ArmClient armClient = new ArmClient(credential);

        ResourceIdentifier resourceGroupId = ResourceGroupResource.CreateResourceIdentifier(subscriptionId, resourceGroup);
        var rg = armClient.GetResourceGroupResource(resourceGroupId);
        var pgServer = await rg.GetPostgreSqlFlexibleServerAsync(server);
        var pgServerData = pgServer.Value.Data;

        var result = new
        {
            server = new
            {
                name = pgServerData.Name,
                location = pgServerData.Location,
                version = pgServerData.Version,
                sku = pgServerData.Sku?.Name,
                storage_profile = new
                {
                    storage_size_gb = pgServerData.Storage?.StorageSizeInGB,
                    backup_retention_days = pgServerData.Backup?.BackupRetentionDays,
                    geo_redundant_backup = pgServerData.Backup?.GeoRedundantBackup
                }
            }
        };

        return JsonSerializer.Serialize(result);
    }

    public async Task<string> GetServerParameterAsync(string subscriptionId, string resourceGroup, string user, string server, string param)
    {
        var credential = new DefaultAzureCredential();
        ArmClient armClient = new ArmClient(credential);

        ResourceIdentifier resourceGroupId = ResourceGroupResource.CreateResourceIdentifier(subscriptionId, resourceGroup);
        var rg = armClient.GetResourceGroupResource(resourceGroupId);

        // Get the PostgreSQL Flexible Server
        var pgServer = await rg.GetPostgreSqlFlexibleServerAsync(server);

        // Get the configuration parameter asynchronously
        var configResponse = await pgServer.Value.GetPostgreSqlFlexibleServerConfigurationAsync(param);

        if (configResponse?.Value?.Data == null)
        {
            throw new ArgumentException($"Parameter '{param}' not found.");
        }

        // Return the param value
        return configResponse.Value.Data.Value;
    }

    public async Task<List<string>> ListServersAsync(string subscriptionId, string resourceGroup, string user)
    {
        var credential = new DefaultAzureCredential();
        ArmClient armClient = new ArmClient(credential);

        ResourceIdentifier resourceGroupId = ResourceGroupResource.CreateResourceIdentifier(subscriptionId, resourceGroup);
        var rg = armClient.GetResourceGroupResource(resourceGroupId);

        List<string> serverList = [];

        await foreach (PostgreSqlFlexibleServerResource server in rg.GetPostgreSqlFlexibleServers().GetAllAsync())
        {
            serverList.Add(server.Data.Name);
        }
        return serverList;
    }
}