// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using AzureMcp.Arguments.Postgres.Database;
using Microsoft.Extensions.Logging;
using AzureMcp.Models.Command;
using ModelContextProtocol.Server;
using AzureMcp.Services.Interfaces;

namespace AzureMcp.Commands.Postgres.Database;

public sealed class DatabaseListCommand(ILogger<DatabaseListCommand> logger) : BasePostgresCommand<DatabaseListArguments>(logger)
{

    protected override string GetCommandName() => "list";

    protected override string GetCommandDescription() => "Lists all databases in the PostgreSQL server.";

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

            var databases = await pgService.ListDatabasesAsync(args.Subscription!, args.ResourceGroup!, args.User!, args.Server!);
            if (databases == null || databases.Count == 0)
            {
                context.Response.Results = new { message = "No databases found." };
                return context.Response;
            }
            context.Response.Results = databases;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred listing databases.");
            HandleException(context.Response, ex);
        }

        return context.Response;
    }
}
