using System.CommandLine;
using System.CommandLine.Parsing;
using AzureMcp.Arguments.Postgres.Database;
using AzureMcp.Models.Argument;
using AzureMcp.Models.Command;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace AzureMcp.Commands.Postgres.Database;

public sealed class DatabaseQueryCommand(ILogger<DatabaseQueryCommand> logger) : BasePostgresCommand<DatabaseQueryArguments>(logger)
{
    protected override string GetCommandName() => "query";

    protected override string GetCommandDescription() => "Executes a query on the PostgreSQL database.";

    protected override void RegisterArguments()
    {
        base.RegisterArguments();
        AddArgument(ArgumentBuilder<DatabaseListArguments>
            .Create(ArgumentDefinitions.Postgres.Server.Name, ArgumentDefinitions.Postgres.Server.Description)
            .WithValueAccessor(args => args.Server ?? string.Empty)
            .WithIsRequired(true));

        AddArgument(ArgumentBuilder<DatabaseQueryArguments>
            .Create(ArgumentDefinitions.Postgres.Database.Name, ArgumentDefinitions.Postgres.Database.Description)
            .WithValueAccessor(args => args.Database ?? string.Empty)
            .WithIsRequired(true));            
                
        AddArgument(ArgumentBuilder<DatabaseQueryArguments>
            .Create(ArgumentDefinitions.Postgres.Query.Name, ArgumentDefinitions.Postgres.Query.Description)
            .WithValueAccessor(args => args.Query ?? string.Empty)
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
            var serverName = args.Server ?? throw new ArgumentNullException(nameof(args.Server), "Server name cannot be null.");
            var user = args.User ?? throw new ArgumentNullException(nameof(args.User), "User cannot be null.");
            var databaseName = args.Database ?? throw new ArgumentNullException(nameof(args.Database), "Database name cannot be null.");
            var query = args.Query ?? throw new ArgumentNullException(nameof(args.Query), "Query cannot be null.");

            var result = await pgService.ExecuteQueryAsync(args.Subscription, args.ResourceGroup, args.Server, args.User, args.Database, args.Query);
            context.Response.Results = new { QueryResult = result };
        }
        catch (Exception ex)
        {
            HandleException(context.Response, ex);
        }

        return context.Response;
    }
}