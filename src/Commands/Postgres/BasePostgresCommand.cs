// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using AzureMcp.Arguments.Postgres;
using AzureMcp.Models.Argument;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Commands.Postgres;

public abstract class BasePostgresCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TArgs>
    : SubscriptionCommand<TArgs> where TArgs : BasePostgresArguments, new()
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
        AddArgument(CreateResourceGroupArgument());
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

    protected ArgumentBuilder<TArgs> CreateUserArgument() =>
        ArgumentBuilder<TArgs>
            .Create(ArgumentDefinitions.Postgres.User.Name, ArgumentDefinitions.Postgres.User.Description)
            .WithValueAccessor(args => args.User ?? string.Empty)
            .WithIsRequired(ArgumentDefinitions.Postgres.User.Required);

    protected virtual ArgumentBuilder<TArgs> CreateServerArgument() =>
        ArgumentBuilder<TArgs>
            .Create(ArgumentDefinitions.Postgres.Server.Name, ArgumentDefinitions.Postgres.Server.Description)
            .WithValueAccessor(args => args.Server ?? string.Empty)
            .WithIsRequired(ArgumentDefinitions.Postgres.Server.Required);

    protected virtual ArgumentBuilder<TArgs> CreateDatabaseArgument() =>
       ArgumentBuilder<TArgs>
           .Create(ArgumentDefinitions.Postgres.Database.Name, ArgumentDefinitions.Postgres.Database.Description)
           .WithValueAccessor(args => args.Database ?? string.Empty)
           .WithIsRequired(ArgumentDefinitions.Postgres.Database.Required);

}
