using System.CommandLine;
using System.CommandLine.Parsing;
using AzureMcp.Arguments.PostgreSQL;
using AzureMcp.Models.Command;
using AzureMcp.Services.Interfaces;

namespace AzureMcp.Commands.PostgreSQL.Server;

public sealed class GetConfigCommand : SubscriptionCommand<GetConfigArguments>
{
    public GetConfigCommand(IPostgreSQLService postgresqlService) : base()
    {
        _postgresqlService = postgresqlService;
    }
    private readonly Option<string> _serverNameOption = new Option<string>("--server-name", "The name of the PostgreSQL server.")
    {
        IsRequired = true
    };
    private IPostgreSQLService _postgresqlService;


    protected override string GetCommandName() => "get-config";

    protected override string GetCommandDescription() =>
        "Retrieve the configuration of a PostgreSQL server.";

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_serverNameOption);
    }

    protected override void RegisterArguments()
    {
        base.RegisterArguments();
    }

    protected override GetConfigArguments BindArguments(ParseResult parseResult)
    {
        var args = base.BindArguments(parseResult);
        args.ServerName = parseResult.GetValueForOption(_serverNameOption);
        return args;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var args = BindArguments(parseResult);

        try
        {
            var service = context.GetService<IPostgreSQLService>();
            var config = await service.GetServerConfigAsync(args.ServerName!, args.Subscription!, args.ResourceGroup);

            context.Response.Results = config;
        }
        catch (Exception ex)
        {
            context.Response.Status = 500;
            context.Response.Message = ex.Message;
        }

        return context.Response;
    }
}