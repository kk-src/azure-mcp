namespace AzureMcp.Arguments.PostgreSQL;

public class DatabaseListArguments : SubscriptionArguments
{
    public new string? ResourceGroup { get; set; }
    public string? Server { get; set; }
    public string? User { get; set; }
}