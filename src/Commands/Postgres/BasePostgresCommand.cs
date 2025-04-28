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
    protected readonly Option<string> _serverOption = ArgumentDefinitions.Postgres.Server.ToOption();
    protected readonly Option<string> _databaseOption = ArgumentDefinitions.Postgres.Database.ToOption();

    protected readonly ILogger<BasePostgresCommand<TArgs>> _logger;

    protected BasePostgresCommand(ILogger<BasePostgresCommand<TArgs>> logger)
    {
        _logger = logger;
    }


    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_resourceGroupOption);
        command.AddOption(_userOption);
        command.AddOption(_serverOption);
        command.AddOption(_databaseOption);
    }

    protected override void RegisterArguments()
    {
        base.RegisterArguments();
        AddArgument(CreateUserArgument());
        AddArgument(CreateServerArgument());
        AddArgument(CreateDatabaseArgument());
    }
    protected override TArgs BindArguments(ParseResult parseResult)
    {
        var args = base.BindArguments(parseResult);
        args.ResourceGroup = parseResult.GetValueForOption(_resourceGroupOption);
        args.User = parseResult.GetValueForOption(_userOption);
        args.Server = parseResult.GetValueForOption(_serverOption);
        args.Database = parseResult.GetValueForOption(_databaseOption);
        return args;
    }

    // Helper methods for creating Postgres-specific arguments

    // Over ride the need to have subscription and resource group as mandatory 
    // parameters. PostgreSQL database can be accessed directly using Npgsql 
    // by using host, user, database and password.
    protected new ArgumentBuilder<TArgs> CreateSubscriptionArgument() =>
        ArgumentBuilder<TArgs>
            .Create(ArgumentDefinitions.Common.Subscription.Name, ArgumentDefinitions.Common.Subscription.Description)
            .WithValueAccessor(args => args.Subscription ?? string.Empty)
            .WithIsRequired(false);
    protected new ArgumentBuilder<TArgs> CreateResourceGroupArgument() =>
        ArgumentBuilder<TArgs>
            .Create(ArgumentDefinitions.Common.ResourceGroup.Name, ArgumentDefinitions.Common.ResourceGroup.Description)
            .WithValueAccessor(args => args.ResourceGroup ?? string.Empty)
            .WithIsRequired(false);

    protected ArgumentBuilder<TArgs> CreateUserArgument() =>
        ArgumentBuilder<TArgs>
            .Create(ArgumentDefinitions.Postgres.User.Name, ArgumentDefinitions.Postgres.User.Description)
            .WithValueAccessor(args => args.User ?? string.Empty)
            .WithIsRequired(ArgumentDefinitions.Postgres.User.Required);

    protected ArgumentBuilder<TArgs> CreateServerArgument() =>
        ArgumentBuilder<TArgs>
            .Create(ArgumentDefinitions.Postgres.Server.Name, ArgumentDefinitions.Postgres.Server.Description)
            .WithValueAccessor(args => args.Server ?? string.Empty)
            .WithIsRequired(ArgumentDefinitions.Postgres.Server.Required);

    protected ArgumentBuilder<TArgs> CreateDatabaseArgument() =>
        ArgumentBuilder<TArgs>
            .Create(ArgumentDefinitions.Postgres.Database.Name, ArgumentDefinitions.Postgres.Database.Description)
            .WithValueAccessor(args => args.Database ?? string.Empty)
            .WithIsRequired(ArgumentDefinitions.Postgres.Database.Required);

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        _ => base.GetErrorMessage(ex)
    };

    protected override int GetStatusCode(Exception ex) => ex switch
    {
        _ => base.GetStatusCode(ex)
    };
}