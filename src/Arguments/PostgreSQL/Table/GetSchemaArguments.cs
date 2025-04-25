namespace AzureMcp.Arguments.PostgreSQL.Table;

public class GetSchemaArguments : SubscriptionArguments
{

    public string? Server { get; set; }
    public string? DatabaseName { get; set; }
    public string? TableName { get; set; }
    public string? User { get; set; }
}