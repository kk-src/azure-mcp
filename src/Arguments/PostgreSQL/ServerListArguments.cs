namespace AzureMcp.Arguments.PostgreSQL;

public class ServerListArguments : GlobalArguments
{
    // No additional arguments are required for this command.
    public string? Subscription { get; set; }
    public string? TenantId { get; set; }
}