using System.CommandLine;
using System.CommandLine.Parsing;
using AzureMcp.Arguments.PostgreSQL;
using AzureMcp.Models.Argument;
using AzureMcp.Models.Command;
using AzureMcp.Services.Interfaces;

namespace AzureMcp.Commands.PostgreSQL;

public sealed class DatabaseQueryCommand : SubscriptionCommand<DatabaseQueryArguments>
{
    private readonly IPostgreSQLService _postgresqlService;

    public DatabaseQueryCommand(IPostgreSQLService postgresqlService) : base()
    {
        _postgresqlService = postgresqlService;
    }

    protected override string GetCommandName() => "query";

    protected override string GetCommandDescription() => "Executes a query on the specified PostgreSQL database.";

    protected override void RegisterArguments()
    {
        base.RegisterArguments();
        AddArgument(ArgumentBuilder<DatabaseQueryArguments>
            .Create("server", "The name of the PostgreSQL server.")
            .WithValueAccessor(args => args.Server ?? string.Empty)
            .WithIsRequired(true));
        AddArgument(ArgumentBuilder<DatabaseQueryArguments>
            .Create("user", "The username for the PostgreSQL server.")
            .WithValueAccessor(args => args.User ?? string.Empty)
            .WithIsRequired(true));
        AddArgument(ArgumentBuilder<DatabaseQueryArguments>
            .Create("query", "The SQL query to execute.")
            .WithValueAccessor(args => args.Query ?? string.Empty)
            .WithIsRequired(true));
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var args = BindArguments(parseResult);

        try
        {
            var result = await _postgresqlService.ExecuteQueryAsync(args.Server!, args.User!, args.Query!);
            context.Response.Results = new { QueryResult = result };
        }
        catch (Exception ex)
        {
            HandleException(context.Response, ex);
        }

        return context.Response;
    }
}