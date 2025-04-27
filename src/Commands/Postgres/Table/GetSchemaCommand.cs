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
    protected override string GetCommandName() => "get-schema";

    protected override string GetCommandDescription() =>
        "Retrieves the schema of a specified table in a PostgreSQL database.";

    protected override void RegisterArguments()
    {
        base.RegisterArguments();

        AddArgument(ArgumentBuilder<GetSchemaArguments>
            .Create(ArgumentDefinitions.Postgres.Server.Name, ArgumentDefinitions.Postgres.Server.Description)
            .WithValueAccessor(args => args.Server ?? string.Empty)
            .WithIsRequired(true));
        AddArgument(ArgumentBuilder<GetSchemaArguments>
            .Create(ArgumentDefinitions.Postgres.Database.Name, ArgumentDefinitions.Postgres.Database.Description)
            .WithValueAccessor(args => args.Database ?? string.Empty)
            .WithIsRequired(true));
        AddArgument(ArgumentBuilder<GetSchemaArguments>
            .Create(ArgumentDefinitions.Postgres.Table.Name, ArgumentDefinitions.Postgres.Table.Description)
            .WithValueAccessor(args => args.Table ?? string.Empty)
            .WithIsRequired(true));
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
            var subscriptionId = args.Subscription ?? throw new ArgumentNullException(nameof(args.Subscription), "Subscription ID cannot be null.");
            var resourceGroup = args.ResourceGroup ?? throw new ArgumentNullException(nameof(args.ResourceGroup), "Resource group cannot be null.");
            var server = args.Server ?? throw new ArgumentNullException(nameof(args.Server), "Server name cannot be null.");
            var user = args.User ?? throw new ArgumentNullException(nameof(args.User), "User cannot be null.");
            var database = args.Database ?? throw new ArgumentNullException(nameof(args.Database), "Database name cannot be null.");
            var table = args.Table ?? throw new ArgumentNullException(nameof(args.Table), "Table name cannot be null.");

            var schema = await pgService.GetTableSchemaAsync(subscriptionId, resourceGroup, server, user, database, table);
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