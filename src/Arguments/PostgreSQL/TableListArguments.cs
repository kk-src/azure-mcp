namespace AzureMcp.Arguments.PostgreSQL;

public class TableListArguments: SubscriptionArguments
{
    public string? Server { get; set; }
    public string? DatabaseName { get; set; }
    public string? User { get; set; } 
}