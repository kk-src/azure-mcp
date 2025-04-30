// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AzureMcp.Arguments.Postgres.Server;

public class ServerListArguments : BasePostgresArguments
{
    public void Validate()
    {
        ValidateProperties([
                (Subscription, nameof(Subscription)),
                (ResourceGroup, nameof(ResourceGroup)),
                (User, nameof(User))
        ]);
    }
}
