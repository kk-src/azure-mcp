using System.CommandLine;
using System.CommandLine.Parsing;
using AzureMcp.Arguments.PostgreSQL;
using AzureMcp.Models.Argument;
using AzureMcp.Models.Command;
using AzureMcp.Services.Interfaces;
using ModelContextProtocol.Server;

namespace AzureMcp.Commands.PostgreSQL;

public sealed class ServerListCommand : GlobalCommand<ServerListArguments>
{
    public ServerListCommand() : base()
    {
        AddArgument(ArgumentDefinitions.Common.Subscription);
    }

    protected override string GetCommandName() => "list";

    protected override string GetCommandDescription() =>
        "Lists all PostgreSQL servers in the specified subscription.";

    protected override ServerListArguments BindArguments(ParseResult parseResult)
    {
        var args = base.BindArguments(parseResult);
        return args;
    }

    [McpServerTool(Destructive = false, ReadOnly = true)]
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var args = BindArguments(parseResult);

        try
        {
            if (string.IsNullOrEmpty(args.Subscription))
            {
                context.Response.Status = 400;
                context.Response.Message = "Subscription ID is required.";
                return context.Response;
            }

            var service = context.GetService<IPostgreSQLService>();
            var servers = await service.ListPostgreSqlServersAsync(args.Subscription!, args.TenantId);

            context.Response.Results = servers?.Count > 0 ? new { servers } : null;
        }
        catch (Exception ex)
        {
            HandleException(context.Response, ex);
        }

        return context.Response;
    }
}