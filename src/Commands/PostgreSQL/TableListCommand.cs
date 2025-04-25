using System.CommandLine;
using System.CommandLine.Parsing;
using AzureMcp.Models.Command;
using AzureMcp.Services.Interfaces;
using AzureMcp.Arguments.PostgreSQL;

namespace AzureMcp.Commands.PostgreSQL;

public sealed class TableListCommand : SubscriptionCommand<TableListArguments>
{
    private readonly IPostgreSQLService _postgresqlService;

    public TableListCommand(IPostgreSQLService postgresqlService) : base()
    {
        _postgresqlService = postgresqlService;
    }
    private readonly Option<string> _serverOption = new Option<string>("--server", "The server name.") { IsRequired = true };
    private readonly Option<string> _databaseOption = new Option<string>("--database-name", "The database name.") { IsRequired = true };
    private readonly Option<string> _userOption = new Option<string>("--user", "The user name for authentication.") { IsRequired = true };

    protected override string GetCommandName() => "list";

    protected override string GetCommandDescription() => "Lists all tables in a specified PostgreSQL database.";

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_serverOption);
        command.AddOption(_databaseOption);
        command.AddOption(_userOption);
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var server = parseResult.GetValueForOption(_serverOption);
        var databaseName = parseResult.GetValueForOption(_databaseOption);
        var user = parseResult.GetValueForOption(_userOption);

        var service = context.GetService<IPostgreSQLService>();

        try
        {
            var tables = await service.ListTablesAsync(server!, databaseName!, user!);
            context.Response.Results = tables;
        }
        catch (Exception ex)
        {
            context.Response.Status = 500;
            context.Response.Message = ex.Message;
        }

        return context.Response;
    }
}