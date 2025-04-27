// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Arguments.Postgres;
using AzureMcp.Models.Argument;
using System.CommandLine;
using System.CommandLine.Parsing;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Commands.Postgres;

public abstract class BasePostgresCommand<TArgs> : SubscriptionCommand<TArgs> where TArgs : BasePostgresArguments, new()
{
    protected readonly Option<string> _userOption = ArgumentDefinitions.Postgres.User.ToOption();
    protected readonly ILogger<BasePostgresCommand<TArgs>> _logger;

    protected BasePostgresCommand(ILogger<BasePostgresCommand<TArgs>> logger)
    {
        _logger = logger;
    }


    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_userOption);
    }

    protected override void RegisterArguments()
    {
        base.RegisterArguments();
        AddArgument(CreateUserArgument());
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {        
        _ => base.GetErrorMessage(ex)
    };

    protected override int GetStatusCode(Exception ex) => ex switch
    {     
        _ => base.GetStatusCode(ex)
    };

    protected override TArgs BindArguments(ParseResult parseResult)
    {
        var args = base.BindArguments(parseResult);
        args.User = parseResult.GetValueForOption(_userOption);
        return args;
    }

    // Helper methods for creating Postgres-specific arguments
    protected ArgumentBuilder<TArgs> CreateUserArgument() =>
        ArgumentBuilder<TArgs>
            .Create(ArgumentDefinitions.Postgres.User.Name, ArgumentDefinitions.Postgres.User.Description)
            .WithValueAccessor(args => args.User ?? string.Empty)
            .WithIsRequired(ArgumentDefinitions.Postgres.User.Required);
}