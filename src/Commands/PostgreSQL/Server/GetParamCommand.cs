using System.CommandLine;
using System.CommandLine.Parsing;
using AzureMcp.Arguments.PostgreSQL.Server;
using AzureMcp.Models.Command;
using AzureMcp.Services.Interfaces;
using ModelContextProtocol.Server;

namespace AzureMcp.Commands.PostgreSQL.Server;

public sealed class GetParamCommand : SubscriptionCommand<GetParamArguments>
{    
    
    public GetParamCommand(IPostgreSQLService postgresqlService) : base()
    {
        _postgresqlService = postgresqlService;
    }
    private readonly Option<string> _serverOption = new Option<string>("--server", "The server name.");
    private readonly Option<string> _parameterOption = new Option<string>("--param", "The parameter name.");
    private IPostgreSQLService _postgresqlService;


    protected override string GetCommandName() => "get-param";

    protected override string GetCommandDescription() =>
        "Retrieves a specific parameter of a PostgreSQL server.";

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_resourceGroupOption);
        command.AddOption(_serverOption);
        command.AddOption(_parameterOption);
    }

    protected override GetParamArguments BindArguments(ParseResult parseResult)
    {
        var args = base.BindArguments(parseResult);
        args.ResourceGroup = parseResult.GetValueForOption(_resourceGroupOption);
        args.Server = parseResult.GetValueForOption(_serverOption);
        args.Parameter = parseResult.GetValueForOption(_parameterOption);
        return args;
    }

    [McpServerTool(Destructive = false, ReadOnly = true)]
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var args = BindArguments(parseResult);

        try
        {
            var service = context.GetService<IPostgreSQLService>();
            var result = await service.GetServerParameterAsync(
                args.Subscription!,
                args.ResourceGroup!,
                args.Server!,
                args.Parameter!);

            context.Response.Results = new { ParameterValue = result };
        }
        catch (Exception ex)
        {
            context.Response.Status = 500;
            context.Response.Message = ex.Message;
        }

        return context.Response;
    }
}