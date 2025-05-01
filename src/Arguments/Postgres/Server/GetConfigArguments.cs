// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AzureMcp.Arguments.Postgres.Server;

public class GetConfigArguments : BasePostgresArguments
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
