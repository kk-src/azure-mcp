namespace AzureMcp.Arguments.Postgres.Database;

public class DatabaseQueryArguments : BaseDatabaseArguments
{ 
    public string? Database { get; set; }
    public string? Query { get; set; }
}