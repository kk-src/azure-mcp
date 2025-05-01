// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using AzureMcp.Arguments.Postgres.Table;
using AzureMcp.Models.Command;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;


namespace AzureMcp.Commands.Postgres.Table;

public sealed class TableListCommand(ILogger<TableListCommand> logger) : BasePostgresCommand<TableListArguments>(logger)
{
    protected override string GetCommandName() => "list";
    protected override string GetCommandDescription() => "Lists all tables in the PostgreSQL database.";

    [McpServerTool(Destructive = false, ReadOnly = true)]
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        try
        {
            var args = BindArguments(parseResult);
            args.Validate();

            if (!await ProcessArguments(context, args))
            {
                return context.Response;
            }

            var pgService = context.GetService<IPostgresService>() ?? throw new InvalidOperationException("PostgreSQL service is not available.");
            var tables = await pgService.ListTablesAsync(args.Subscription!, args.ResourceGroup!, args.User!, args.Server!, args.Database!);
            if (tables == null || tables.Count == 0)
            {
                context.Response.Results = null;
                return context.Response;
            }
            context.Response.Results = tables;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred listing tables.");
            HandleException(context.Response, ex);
        }
        return context.Response;
    }
}
