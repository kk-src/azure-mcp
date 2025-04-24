using System.CommandLine;
using System.CommandLine.Parsing;
using AzureMcp.Arguments.PostgreSQL;
using AzureMcp.Models.Argument;
using AzureMcp.Models.Command;
using AzureMcp.Services.Interfaces;

namespace AzureMcp.Commands.PostgreSQL;

public sealed class DatabaseListCommand : SubscriptionCommand<DatabaseListArguments>
{
    private readonly IPostgreSQLService _postgresqlService;

    public DatabaseListCommand(IPostgreSQLService postgresqlService)
    {
        _postgresqlService = postgresqlService;
    }

    protected override string GetCommandName() => "list";

    protected override string GetCommandDescription() => "Lists all PostgreSQL databases.";

    protected override void RegisterArguments()
    {
        base.RegisterArguments();
        AddArgument(ArgumentBuilder<DatabaseListArguments>
            .Create("resource-group", "The name of the resource group.")
            .WithValueAccessor(args => args.ResourceGroup ?? string.Empty)
            .WithIsRequired(false));

        AddArgument(ArgumentBuilder<DatabaseListArguments>
            .Create("server", "The name of the PostgreSQL server.")
            .WithValueAccessor(args => args.Server ?? string.Empty)
            .WithIsRequired(false));

        AddArgument(ArgumentBuilder<DatabaseListArguments>
            .Create("user", "The username for the PostgreSQL server.")
            .WithValueAccessor(args => args.User ?? string.Empty)
            .WithIsRequired(false));
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        try
        {
            var databases = await _postgresqlService.ListDatabasesAsync();
            context.Response.Results = databases;
        }
        catch (Exception ex)
        {
            HandleException(context.Response, ex);
        }

        return context.Response;
    }
}