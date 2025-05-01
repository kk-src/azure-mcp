using AzureMcp.Commands.Postgres.Table;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.CommandLine.Parsing;
using System.Text.Json;
using Xunit;
using AzureMcp.Models.Command;
using System.CommandLine;

namespace AzureMcp.Tests.Commands.Postgres.Table;

public class TableListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPostgresService _postgresService;
    private readonly ILogger<TableListCommand> _logger;

    public TableListCommandTests()
    {
        _postgresService = Substitute.For<IPostgresService>();
        _logger = Substitute.For<ILogger<TableListCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_postgresService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsTables_WhenTablesExist()
    {
        var expectedTables = new List<string> { "table1", "table2" };
        _postgresService.ListTablesAsync("sub123", "rg1", "user1", "server1", "db123").Returns(expectedTables);

        var command = new TableListCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "sub123", "--resource-group", "rg1", "--user", "user1", "--server", "server1", "--database", "db123"]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(expectedTables, response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyList_WhenNoTablesExist()
    {
        _postgresService.ListTablesAsync("sub123", "rg1", "user1", "server1", "db123").Returns([]);

        var command = new TableListCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "sub123", "--resource-group", "rg1", "--user", "user1", "--server", "server1", "--database", "db123"]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Null(response.Results);
    }
}