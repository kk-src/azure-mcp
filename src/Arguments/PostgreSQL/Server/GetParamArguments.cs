namespace AzureMcp.Arguments.PostgreSQL.Server;

public class GetParamArguments : SubscriptionArguments
{
    public string? Server { get; set; }
    public string? Parameter { get; set; }
}