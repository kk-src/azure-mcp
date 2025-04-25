using System.CommandLine;
using System.CommandLine.Parsing;
using AzureMcp.Arguments.PostgreSQL.Table;
using AzureMcp.Models.Command;
using AzureMcp.Services.Interfaces;
using ModelContextProtocol.Server;

namespace AzureMcp.Commands.PostgreSQL.Table;

public sealed class GetSchemaCommand : SubscriptionCommand<GetSchemaArguments>
{
    private readonly IPostgreSQLService _postgresqlService;

    public GetSchemaCommand(IPostgreSQLService postgresqlService) : base()
    {
        _postgresqlService = postgresqlService;
    }
    private readonly Option<string> _serverOption = new Option<string>("--server", "The server name.") { IsRequired = true };
    private readonly Option<string> _databaseOption = new Option<string>("--database-name", "The database name.") { IsRequired = true };
    private readonly Option<string> _userOption = new Option<string>("--user", "The user name for authentication.") { IsRequired = true };
    private readonly Option<string> _tableOption = new Option<string>("--table", "The user name for authentication.") { IsRequired = true };

    protected override string GetCommandName() => "get-schema";

    protected override string GetCommandDescription() =>
        "Retrieves the schema of a specified table in a PostgreSQL database.";

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_resourceGroupOption);
        command.AddOption(_serverOption);
        command.AddOption(_databaseOption);
        command.AddOption(_tableOption);
        command.AddOption(_userOption);
    }

    [McpServerTool(Destructive = false, ReadOnly = true)]
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var args = new GetSchemaArguments
        {
            ResourceGroup = parseResult.GetValueForOption(_resourceGroupOption),
            Server = parseResult.GetValueForOption(_serverOption),
            DatabaseName = parseResult.GetValueForOption(_databaseOption),
            TableName = parseResult.GetValueForOption(_tableOption),
            User = parseResult.GetValueForOption(_userOption)
        };

        try
        {
            var service = context.GetService<IPostgreSQLService>();
            var schema = await service.GetTableSchemaAsync(
                args.Server!,
                args.DatabaseName!,
                args.TableName!,
                args.User!);

            context.Response.Results = schema;
        }
        catch (Exception ex)
        {
            HandleException(context.Response, ex);
        }

        return context.Response;
    }
}