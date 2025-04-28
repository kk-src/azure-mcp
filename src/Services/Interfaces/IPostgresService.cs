// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;

namespace AzureMcp.Services.Interfaces;

public interface IPostgresService
{
    /// <summary>
    /// Lists all databases in the specified PostgreSQL server.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID.</param>
    /// <param name="resourceGroup">The resource group name.</param>
    /// <param name="server">The server name.</param>
    /// <param name="user">The user name for authentication.</param>
    /// <returns>A list of database names.</returns>    
    Task<List<string>> ListDatabasesAsync(string subscriptionId, string resourceGroup, string server, string user);

    /// <summary>
    /// Executes a query on the specified PostgreSQL database.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID.</param>
    /// <param name="resourceGroup">The resource group name.</param>
    /// <param name="server">The server name.</param>
    /// <param name="user">The user name for authentication.</param>
    /// <param name="database">The database name.</param>
    /// <param name="query">The SQL query to execute.</param> 
    /// <returns>The result of the query execution.</returns>
    /// <remarks>
    /// This method uses the Npgsql library to connect to the PostgreSQL database and execute the query.  
    Task<string> ExecuteQueryAsync(string subscriptionId, string resourceGroup, string server, string user, string database, string query);

    /// <summary>
    /// Lists all tables in a specified PostgreSQL database.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID.</param>
    /// <param name="resourceGroup">The resource group name.</param>
    /// <param name="server">The server name.</param>
    /// <param name="user">The user name for authentication.</param>
    /// <param name="database">The database name.</param>
    /// <returns>A list of table names.</returns>    
    Task<List<string>> ListTablesAsync(string subscriptionId, string resourceGroup, string server, string user, string database);

    /// <summary>
    /// Retrieves the schema of a specified table in a PostgreSQL database.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID.</param>
    /// <param name="resourceGroup">The resource group name.</param>
    /// <param name="server">The server name.</param>
    /// <param name="user">The user name for authentication.</param>
    /// <param name="database">The database name.</param>
    /// <param name="table">The table name.</param>
    /// <returns>A list of column names and their types.</returns>
    /// <remarks>
    /// This method uses the Npgsql library to connect to the PostgreSQL database and retrieve the schema information.    

    Task<List<string>> GetTableSchemaAsync(string subscriptionId, string resourceGroup, string server, string user, string database, string table);

    Task<string> GetServerConfigAsync(string subscriptionId, string resourceGroup, string user, string server);

    Task<string> GetServerParameterAsync(string subscriptionId, string resourceGroup, string user, string server, string param);

    Task<List<string>> ListServersAsync(string subscriptionId, string resourceGroup, string user);
}
