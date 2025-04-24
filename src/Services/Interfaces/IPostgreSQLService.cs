// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;

namespace AzureMcp.Services.Interfaces;

public interface IPostgreSQLService
{
    Task<List<string>> ListDatabasesAsync();
}