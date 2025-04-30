// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AzureMcp.Arguments.Postgres.Database;

public class DatabaseListArguments : BasePostgresArguments
{
    public void Validate()
    {
        ValidateProperties(
        [
            (Subscription, nameof(Subscription)),
            (ResourceGroup, nameof(ResourceGroup)),
            (User, nameof(User)),
            (Server, nameof(Server))
        ]);
    }
}
