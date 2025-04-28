using System.CommandLine;
using System.CommandLine.Parsing;
using AzureMcp.Arguments.Postgres.Server;
using AzureMcp.Models.Argument;

using AzureMcp.Models.Command;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace AzureMcp.Commands.Postgres.Server;

public sealed class GetParamCommand(ILogger<GetParamCommand> logger) : BasePostgresCommand<GetParamArguments>(logger)
{
    private readonly Option<string> _paramOption = ArgumentDefinitions.Postgres.Param.ToOption();
    protected override string GetCommandName() => "get-param";

    protected override string GetCommandDescription() =>
        "Retrieves a specific parameter of a PostgreSQL server.";

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_paramOption);
    }

    protected override void RegisterArguments()
    {
        base.RegisterArguments();
        AddArgument(ArgumentBuilder<GetParamArguments>
            .Create(ArgumentDefinitions.Postgres.Param.Name, ArgumentDefinitions.Postgres.Param.Description)
            .WithValueAccessor(args => args.Param ?? string.Empty)
            .WithIsRequired(true));
    }

    protected override GetParamArguments BindArguments(ParseResult parseResult)
    {
        var args = base.BindArguments(parseResult);
        args.Param = parseResult.GetValueForOption(_paramOption);
        return args;
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
            var user = args.User ?? throw new ArgumentNullException(nameof(args.User), "User cannot be null.");
            var server = args.Server ?? throw new ArgumentNullException(nameof(args.Server), "Server name cannot be null.");
            var param = args.Param ?? throw new ArgumentNullException(nameof(args.Param), "Parameter cannot be null.");

            var parameterValue = await pgService.GetServerParameterAsync(subscriptionId, resourceGroup, user, server, param);
            if (string.IsNullOrEmpty(parameterValue))
            {
                context.Response.Results = new { message = $"Parameter '{param}' not found." };
                return context.Response;
            }

            context.Response.Results = parameterValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred retrieving the parameter. Server: {Server}.", args.Server);
            HandleException(context.Response, ex);
        }
        return context.Response;
    }
}