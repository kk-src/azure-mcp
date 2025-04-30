// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using AzureMcp.Arguments.Postgres.Table;
using AzureMcp.Models.Argument;
using AzureMcp.Models.Command;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace AzureMcp.Commands.Postgres.Table;

public sealed class GetSchemaCommand(ILogger<GetSchemaCommand> logger) : BasePostgresCommand<GetSchemaArguments>(logger)
{
    private readonly Option<string> _tableOption = ArgumentDefinitions.Postgres.Table.ToOption();
    protected override string GetCommandName() => "get-schema";

    protected override string GetCommandDescription() =>
        "Retrieves the schema of a specified table in a PostgreSQL database.";

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_tableOption);
    }

    protected override void RegisterArguments()
    {
        base.RegisterArguments();
        AddArgument(ArgumentBuilder<GetSchemaArguments>
            .Create(ArgumentDefinitions.Postgres.Table.Name, ArgumentDefinitions.Postgres.Table.Description)
            .WithValueAccessor(args => args.Table ?? string.Empty)
            .WithIsRequired(true));
    }

    protected override GetSchemaArguments BindArguments(ParseResult parseResult)
    {
        var args = base.BindArguments(parseResult);
        args.Table = parseResult.GetValueForOption(_tableOption);
        return args;
    }


    [McpServerTool(Destructive = false, ReadOnly = true)]
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var args = BindArguments(parseResult);
        try
        {
            if (!await ProcessArguments(context, args))
            {
                return context.Response;
            }

            var pgService = context.GetService<IPostgresService>() ?? throw new InvalidOperationException("PostgreSQL service is not available.");


            var schema = await pgService.GetTableSchemaAsync(args.Subscription!, args.ResourceGroup!, args.User!, args.Server!, args.Database!, args.Table!);
            if (schema == null)
            {
                context.Response.Results = new { message = "No schema found." };
                return context.Response;
            }

            context.Response.Results = schema;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred retrieving the schema for table {Table}.", args.Table);
            HandleException(context.Response, ex);
        }

        return context.Response;
    }
}
