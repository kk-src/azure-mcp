namespace AzureMcp.Arguments.Postgres.Table;

public class BaseTableArguments : BasePostgresArguments
{
       public string? Database { get; set; }
       public string? Server { get; set; }
}