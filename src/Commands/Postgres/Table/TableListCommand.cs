using System.CommandLine;
using System.CommandLine.Parsing;
using AzureMcp.Arguments.Postgres.Table;
using AzureMcp.Models.Argument;
using AzureMcp.Models.Command;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;


namespace AzureMcp.Commands.Postgres.Table;

public sealed class TableListCommand(ILogger<TableListCommand> logger) : BasePostgresCommand<TableListArguments>(logger)
{
    protected override string GetCommandName() => "list";
    protected override string GetCommandDescription() => "Lists all tables in the PostgreSQL database.";

    protected override void RegisterArguments()
    {
        base.RegisterArguments();
        
        AddArgument(ArgumentBuilder<TableListArguments>
            .Create(ArgumentDefinitions.Postgres.Server.Name, ArgumentDefinitions.Postgres.Server.Description)
            .WithValueAccessor(args => args.Server ?? string.Empty)
            .WithIsRequired(true));  
        AddArgument(ArgumentBuilder<TableListArguments>
            .Create(ArgumentDefinitions.Postgres.Database.Name, ArgumentDefinitions.Postgres.Database.Description)
            .WithValueAccessor(args => args.Database ?? string.Empty)
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

            var tables = await pgService.ListTablesAsync(args.Subscription, resourceGroup, server, user, database);
            if (tables == null || tables.Count == 0)
            {
                context.Response.Results = new { message = "No tables found." };
                return context.Response;
            }
            context.Response.Results = tables;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred listing tables. Server: {Server}, Database: {Database}.", args.Server, args.Database);
            HandleException(context.Response, ex);            
        }        
        return context.Response;
    }
}