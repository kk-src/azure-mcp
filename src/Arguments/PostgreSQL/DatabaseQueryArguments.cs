namespace AzureMcp.Arguments.PostgreSQL;

public class DatabaseQueryArguments : SubscriptionArguments
{
    public string? Server { get; set; }
    public string? User { get; set; }
    public string? Query { get; set; }
}