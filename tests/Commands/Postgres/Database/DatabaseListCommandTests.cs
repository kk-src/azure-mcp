using AzureMcp.Commands.Postgres.Database;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.CommandLine.Parsing;
using Xunit;
using AzureMcp.Models.Command;
using NSubstitute.ExceptionExtensions;

namespace AzureMcp.Tests.Commands.Postgres.Database;

public class DatabaseListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPostgresService _postgresService;
    private readonly ILogger<DatabaseListCommand> _logger;

    public DatabaseListCommandTests()
    {
        _postgresService = Substitute.For<IPostgresService>();
        _logger = Substitute.For<ILogger<DatabaseListCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_postgresService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsDatabases_WhenDatabasesExist()
    {
        var expectedDatabases = new List<string> { "db1", "db2" };
        _postgresService.ListDatabasesAsync("sub123", "rg1", "user1", "server1").Returns(expectedDatabases);

        var command = new DatabaseListCommand(_logger);
        var parser = new Parser(command.GetCommand());
        var args = parser.Parse(["--subscription", "sub123", "--resource-group", "rg1", "--user", "user1", "--server", "server1"]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(expectedDatabases, response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsMessage_WhenNoDatabasesExist()
    {
        _postgresService.ListDatabasesAsync("sub123", "rg1", "user1", "server1").Returns([]);

        var command = new DatabaseListCommand(_logger);
        var parser = new Parser(command.GetCommand());
        var args = parser.Parse(["--subscription", "sub123", "--resource-group", "rg1", "--user", "user1", "--server", "server1"]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Null(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        _postgresService.ListDatabasesAsync("sub123", "rg1", "user1", "server1").ThrowsAsync(new Exception("Test exception"));

        var command = new DatabaseListCommand(_logger);
        var parser = new Parser(command.GetCommand());
        var args = parser.Parse(["--subscription", "sub123", "--resource-group", "rg1", "--user", "user1", "--server", "server1"]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Contains("Test exception", response.Message);
    }
}