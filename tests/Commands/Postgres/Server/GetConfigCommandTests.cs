using AzureMcp.Commands.Postgres.Server;
using AzureMcp.Models.Command;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.CommandLine;
using System.CommandLine.Parsing;
using Xunit;

namespace AzureMcp.Tests.Commands.Postgres.Server;

public class GetConfigCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPostgresService _postgresService;
    private readonly ILogger<GetConfigCommand> _logger;

    public GetConfigCommandTests()
    {
        _postgresService = Substitute.For<IPostgresService>();
        _logger = Substitute.For<ILogger<GetConfigCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_postgresService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsConfig_WhenConfigExists()
    {
        var expectedConfig = "config123";
        _postgresService.GetServerConfigAsync("sub123", "rg1", "user1", "server123").Returns(expectedConfig);

        var command = new GetConfigCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "sub123", "--resource-group", "rg1", "--user", "user1", "--server", "server123"]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(expectedConfig, response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNull_WhenConfigDoesNotExist()
    {
        _postgresService.GetServerConfigAsync("sub123", "rg1", "user1", "server123").Returns("");

        var command = new GetConfigCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "sub123", "--resource-group", "rg1", "--user", "user1", "--server", "server123"]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Null(response.Results);
    }
}