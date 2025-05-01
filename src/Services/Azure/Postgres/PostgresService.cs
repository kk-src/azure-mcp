// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.PostgreSql.FlexibleServers;
using Azure.ResourceManager.Resources;
using AzureMcp.Services.Interfaces;
using Npgsql;
using System.Text.Json;

namespace AzureMcp.Services.Azure.Postgres;

public class PostgresService : IPostgresService
{
    private readonly ArmClient _armClient;
    private readonly TokenCredential _tokenCredential;
    private string? _cachedAccessToken;
    private DateTime _tokenExpiryTime;

    public PostgresService()
    {
        _tokenCredential = new DefaultAzureCredential();
        _armClient = new ArmClient(_tokenCredential);
    }

    private async Task<string> GetAccessTokenAsync()
    {
        if (_cachedAccessToken != null && DateTime.UtcNow < _tokenExpiryTime)
        {
            return _cachedAccessToken;
        }

        var tokenRequestContext = new TokenRequestContext(new[] { "https://ossrdbms-aad.database.windows.net/.default" });
        var accessToken = await _tokenCredential.GetTokenAsync(tokenRequestContext, CancellationToken.None);

        _cachedAccessToken = accessToken.Token;
        _tokenExpiryTime = accessToken.ExpiresOn.UtcDateTime.AddSeconds(-60); // Subtract 60 seconds as a buffer.

        return _cachedAccessToken;
    }

    private static string NormalizeServerName(string server)
    {
        if (!server.Contains('.'))
        {
            return server + ".postgres.database.azure.com";
        }
        return server;
    }

    public async Task<List<string>> ListDatabasesAsync(string subscriptionId, string resourceGroup, string user, string server)
    {
        var accessToken = await GetAccessTokenAsync();

        var host = NormalizeServerName(server);
        var connectionString = $"Host={host};Database=postgres;Username={user};Password={accessToken};";

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

    public async Task<string> ExecuteQueryAsync(string subscriptionId, string resourceGroup, string user, string server, string database, string query)
    {
        var accessToken = await GetAccessTokenAsync();

        var host = NormalizeServerName(server);
        var connectionString = $"Host={host};Database={database};Username={user};Password={accessToken};";
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

    public async Task<List<string>> ListTablesAsync(string subscriptionId, string resourceGroup, string user, string server, string database)
    {
        var accessToken = await GetAccessTokenAsync();

        var host = NormalizeServerName(server);
        var connectionString = $"Host={host};Database={database};Username={user};Password={accessToken};";
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

    public async Task<List<string>> GetTableSchemaAsync(string subscriptionId, string resourceGroup, string user, string server, string database, string table)
    {
        var accessToken = await GetAccessTokenAsync();

        var host = NormalizeServerName(server);
        var connectionString = $"Host={host};Database={database};Username={user};Password={accessToken};";

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

    public async Task<List<string>> ListServersAsync(string subscriptionId, string resourceGroup, string user)
    {
        ResourceIdentifier resourceGroupId = ResourceGroupResource.CreateResourceIdentifier(subscriptionId, resourceGroup);
        var rg = _armClient.GetResourceGroupResource(resourceGroupId);

        var serverList = new List<string>();

        await foreach (PostgreSqlFlexibleServerResource server in rg.GetPostgreSqlFlexibleServers().GetAllAsync())
        {
            serverList.Add(server.Data.Name);
        }
        return serverList;

    }

    public async Task<string> GetServerConfigAsync(string subscriptionId, string resourceGroup, string user, string server)
    {
        ResourceIdentifier resourceGroupId = ResourceGroupResource.CreateResourceIdentifier(subscriptionId, resourceGroup);
        var rg = _armClient.GetResourceGroupResource(resourceGroupId);
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
        ResourceIdentifier resourceGroupId = ResourceGroupResource.CreateResourceIdentifier(subscriptionId, resourceGroup);
        var rg = _armClient.GetResourceGroupResource(resourceGroupId);

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
}