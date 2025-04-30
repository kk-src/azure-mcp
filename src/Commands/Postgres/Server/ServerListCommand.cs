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
            var subscriptionId = args.Subscription ?? throw new ArgumentNullException(nameof(args.Subscription), "Subscription ID cannot be null.");
            var resourceGroup = args.ResourceGroup ?? throw new ArgumentNullException(nameof(args.ResourceGroup), "Resource group cannot be null.");
            var user = args.User ?? throw new ArgumentNullException(nameof(args.User), "User cannot be null.");

            var servers = await pgService.ListServersAsync(subscriptionId, resourceGroup, user);
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
