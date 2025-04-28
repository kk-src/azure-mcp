// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AzureMcp.Arguments.Postgres;

public class BasePostgresArguments : SubscriptionArguments
{
    public string? User { get; set; }
    public string? Server { get; set; }
    public string? Database { get; set; }
}
