using System.CommandLine;
using System.CommandLine.Parsing;
using AzureMcp.Arguments.Postgres.Server;
using AzureMcp.Models.Argument;
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
            var server = args.Server ?? throw new ArgumentNullException(nameof(args.Server), "Server name cannot be null.");

            var config = await pgService.GetServerConfigAsync(subscriptionId, resourceGroup, user, server);
            if (config == null)
            {
                context.Response.Results = new { message = "No configuration found." };
                return context.Response;
            }
            context.Response.Results = config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred retrieving server configuration. Server: {Server}.", args.Server);
            HandleException(context.Response, ex);
        }


        return context.Response;
    }
}