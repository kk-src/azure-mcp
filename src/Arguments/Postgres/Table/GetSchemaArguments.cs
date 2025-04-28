// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AzureMcp.Arguments.Postgres.Table;

public class GetSchemaArguments : BasePostgresArguments
{
    public string? Table { get; set; }    
}
