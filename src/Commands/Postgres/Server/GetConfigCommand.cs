// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using AzureMcp.Arguments.Postgres.Server;
using AzureMcp.Models.Command;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace AzureMcp.Commands.Postgres.Server;

public sealed class GetConfigCommand(ILogger<GetConfigCommand> logger) : BasePostgresCommand<GetConfigArguments>(logger)
{
    protected override string GetCommandName() => "get-config";
    protected override string GetCommandDescription() =>
        "Retrieve the configuration of a PostgreSQL server.";

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
            var config = await pgService.GetServerConfigAsync(args.Subscription!, args.ResourceGroup!, args.User!, args.Server!);
            if (config == null)
            {
                context.Response.Results = new { message = "No configuration found." };
                return context.Response;
            }
            context.Response.Results = config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred retrieving server configuration.");
            HandleException(context.Response, ex);
        }


        return context.Response;
    }
}
