// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AzureMcp.Arguments.Postgres.Database;

public class DatabaseQueryArguments : BasePostgresArguments
{
    public string? Query { get; set; }
}
