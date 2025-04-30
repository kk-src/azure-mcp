// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AzureMcp.Arguments.Postgres.Table;

public class TableListArguments : BasePostgresArguments
{
    public void Validate()
    {
        ValidateProperties(new[]
        {
            (Subscription, nameof(Subscription)),
            (ResourceGroup, nameof(ResourceGroup)),
            (User, nameof(User)),
            (Server, nameof(Server)),
            (Database, nameof(Database))
        });
    }
}
