namespace AzureMcp.Arguments.PostgreSQL;

public class DatabaseListArguments : SubscriptionArguments
{
    public string? Server { get; set; }
    public string? User { get; set; }
}