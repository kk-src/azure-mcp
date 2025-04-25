// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;

namespace AzureMcp.Services.Interfaces;

public interface IPostgreSQLService
{
    /// <summary>
    /// Lists all PostgreSQL servers in the specified subscription.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID.</param>
    /// <param name="tenantId">Optional tenant ID for cross-tenant operations.</param>
    /// <returns>A list of PostgreSQL server names.</returns>
    Task<List<string>> ListPostgreSqlServersAsync(string subscriptionId, string? tenantId = null);

    Task<List<string>> ListDatabasesAsync();
    Task<string> ExecuteQueryAsync(string server, string user, string query);
    Task<string> GetServerConfigAsync(string serverName, string subscriptionId, string? tenantId = null);
    /// <summary>
    /// Retrieves a specific parameter of a PostgreSQL server.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID.</param>
    /// <param name="resourceGroup">The resource group name.</param>
    /// <param name="serverName">The server name.</param>
    /// <param name="parameterName">The parameter name.</param>
    /// <param name="tenantId">Optional tenant ID for cross-tenant operations.</param>
    /// <returns>The value of the specified parameter.</returns>
    Task<string> GetServerParameterAsync(string subscriptionId, string resourceGroup, string serverName, string parameterName, string? tenantId = null);

    /// <summary>
    /// Lists all tables in a specified PostgreSQL database.
    /// </summary>
    /// <param name="server">The server name.</param>
    /// <param name="databaseName">The database name.</param>
    /// <param name="user">The user name for authentication.</param>
    /// <returns>A list of table names.</returns>
    Task<List<string>> ListTablesAsync(string server, string databaseName, string user);

    /// <summary>
    /// Retrieves the schema of a specified table in a PostgreSQL database.
    /// </summary>
    /// <param name="server">The server name.</param>
    /// <param name="databaseName">The database name.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="user">The user name for authentication.</param>
    /// <returns>The schema of the specified table as a JSON string.</returns>
    Task<string> GetTableSchemaAsync(string server, string databaseName, string tableName, string user);
}