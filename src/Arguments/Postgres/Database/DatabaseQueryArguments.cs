// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AzureMcp.Arguments.Postgres.Database;

public class DatabaseQueryArguments : BasePostgresArguments
{
    public string? Query { get; set; }

    public void Validate()
    {
        ValidateProperties(
        [
            (Subscription, nameof(Subscription)),
            (ResourceGroup, nameof(ResourceGroup)),
            (User, nameof(User)),
            (Server, nameof(Server)),
            (Database, nameof(Database)),
            (Query, nameof(Query))
        ]);
    }
}
