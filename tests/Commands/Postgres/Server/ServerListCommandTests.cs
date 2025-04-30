using AzureMcp.Arguments.Postgres.Server;
using AzureMcp.Commands.Postgres.Server;
using AzureMcp.Models.Command;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.CommandLine.Parsing;
using System.Net.Http;
using System.Text.Json;
using Xunit;

namespace AzureMcp.Tests.Commands.Postgres.Server;

public class ServerListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPostgresService _postgresService;
    private readonly ILogger<ServerListCommand> _logger;

    public ServerListCommandTests()
    {
        _postgresService = Substitute.For<IPostgresService>();
        _logger = Substitute.For<ILogger<ServerListCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_postgresService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsServers_WhenServersExist()
    {
        // Arrange
        var expectedServers = new List<string> { "server1", "server2" };
        _postgresService.ListServersAsync(Arg.Is("sub123"), Arg.Is("rg1"), Arg.Is("user1"))
            .Returns(expectedServers);

        var command = new ServerListCommand(_logger);
        var parser = new Parser(command.GetCommand());
        var args = parser.Parse(new[] { "--subscription", "sub123", "--resource-group", "rg1", "--user", "user1" });
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(expectedServers, response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNull_WhenNoServers()
    {
        // Arrange
        _postgresService.ListServersAsync("sub123", "rg1", "user1")
            .Returns(new List<string>());

        var command = new ServerListCommand(_logger);
        var parser = new Parser(command.GetCommand());
        var args = parser.Parse(new[] { "--subscription", "sub123", "--resource-group", "rg1", "--user", "user1" });
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(JsonSerializer.Serialize(new { message = "No servers found." }), JsonSerializer.Serialize(response.Results));
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";
        _postgresService.ListServersAsync("sub123", "rg1", "user1")
            .ThrowsAsync(new Exception(expectedError));

        var command = new ServerListCommand(_logger);
        var parser = new Parser(command.GetCommand());
        var args = parser.Parse(new[] { "--subscription", "sub123", "--resource-group", "rg1", "--user", "user1" });
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.Equal(expectedError, response.Message);
    }
}