// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using AzureMcp.Arguments.Postgres.Server;
using AzureMcp.Models.Command;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace AzureMcp.Commands.Postgres.Server;

public sealed class ServerListCommand(ILogger<ServerListCommand> logger) : BasePostgresCommand<ServerListArguments>(logger)
{
    protected override string GetCommandName() => "list";

    protected override string GetCommandDescription() =>
        "Lists all PostgreSQL servers in the specified subscription.";

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
    
            args.Validate();
            var servers = await pgService.ListServersAsync(args.Subscription!, args.ResourceGroup!, args.User!);
            if (servers == null || servers.Count == 0)
            {
                context.Response.Results = new { message = "No servers found." };
                return context.Response;
            }

            context.Response.Results = servers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred listing servers. Subscription: {Subscription}.", args.Subscription);
            HandleException(context.Response, ex);
        }

        return context.Response;
    }
}
