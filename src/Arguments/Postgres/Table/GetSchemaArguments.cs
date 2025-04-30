// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AzureMcp.Arguments.Postgres.Table;

public class GetSchemaArguments : BasePostgresArguments
{
    public string? Table { get; set; }

    public void Validate()
    {
        ValidateProperties(
        [
            (Subscription, nameof(Subscription)),
            (ResourceGroup, nameof(ResourceGroup)),
            (User, nameof(User)),
            (Server, nameof(Server)),
            (Database, nameof(Database)),
            (Table, nameof(Table))
        ]);
    }
}
