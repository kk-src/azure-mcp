// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AzureMcp.Arguments.Postgres;

public class BasePostgresArguments : SubscriptionArguments
{
    public string? User { get; set; }
    public string? Server { get; set; }
    public string? Database { get; set; }

    protected static void ValidateProperties((string? Property, string Name)[] properties)
    {
        foreach (var (property, name) in properties)
        {
            if (string.IsNullOrEmpty(property))
            {
                throw new ArgumentNullException(name, $"{name} cannot be null or empty.");
            }
        }
    }
}
