// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AzureMcp.Arguments.Postgres.Server;

public class GetParamArguments : BasePostgresArguments
{
    public string? Param { get; set; }
}
