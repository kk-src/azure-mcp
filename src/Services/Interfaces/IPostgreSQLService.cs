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
}