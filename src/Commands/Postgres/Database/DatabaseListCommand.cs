using System.CommandLine.Parsing;
using AzureMcp.Arguments.Postgres.Database;
using Microsoft.Extensions.Logging;
using AzureMcp.Models.Argument;
using AzureMcp.Models.Command;
using ModelContextProtocol.Server;
using AzureMcp.Services.Interfaces;
using System.CommandLine;

namespace AzureMcp.Commands.Postgres.Database;

public sealed class DatabaseListCommand(ILogger<DatabaseListCommand> logger) : BasePostgresCommand<DatabaseListArguments>(logger)
{

    protected override string GetCommandName() => "list";

    protected override string GetCommandDescription() => "Lists all databases in the PostgreSQL server.";

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
            var serverName = args.Server ?? throw new ArgumentNullException(nameof(args.Server), "Server name cannot be null.");
            var user = args.User ?? throw new ArgumentNullException(nameof(args.User), "User cannot be null.");

            var databases = await pgService.ListDatabasesAsync(args.Subscription, resourceGroup, args.Server, args.User);
            if (databases == null || databases.Count == 0)
            {
                context.Response.Results = new { message = "No databases found." };
                return context.Response;
            }
            context.Response.Results = databases;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred listing databases. Server: {Server}.", args.Server);
            HandleException(context.Response, ex);
        }

        return context.Response;
    }
}