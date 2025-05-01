// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using AzureMcp.Arguments.Postgres.Database;
using AzureMcp.Models.Argument;
using AzureMcp.Models.Command;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace AzureMcp.Commands.Postgres.Database;

public sealed class DatabaseQueryCommand(ILogger<DatabaseQueryCommand> logger) : BasePostgresCommand<DatabaseQueryArguments>(logger)
{
    private readonly Option<string> _queryOption = ArgumentDefinitions.Postgres.Query.ToOption();
    protected override string GetCommandName() => "query";

    protected override string GetCommandDescription() => "Executes a query on the PostgreSQL database.";

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_queryOption);
    }

    protected override void RegisterArguments()
    {
        base.RegisterArguments();
        AddArgument(CreateQueryArgument());
    }

    protected override DatabaseQueryArguments BindArguments(ParseResult parseResult)
    {
        var args = base.BindArguments(parseResult);
        args.Query = parseResult.GetValueForOption(_queryOption);
        return args;
    }


    [McpServerTool(Destructive = false, ReadOnly = true)]
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        try
        {
            var args = BindArguments(parseResult);
            if (!await ProcessArguments(context, args))
            {
                return context.Response;
            }
            var pgService = context.GetService<IPostgresService>() ?? throw new InvalidOperationException("PostgreSQL service is not available.");



            args.Validate();

            var result = await pgService.ExecuteQueryAsync(args.Subscription!, args.ResourceGroup!, args.Server!, args.User!, args.Database!, args.Query!);
            context.Response.Results = new { QueryResult = result };
        }
        catch (Exception ex)
        {
            HandleException(context.Response, ex);
        }

        return context.Response;
    }

    private static ArgumentBuilder<DatabaseQueryArguments> CreateQueryArgument() =>
        ArgumentBuilder<DatabaseQueryArguments>
            .Create(ArgumentDefinitions.Postgres.Query.Name, ArgumentDefinitions.Postgres.Query.Description)
            .WithValueAccessor(args => args.Query ?? string.Empty)
            .WithIsRequired(true);
}
