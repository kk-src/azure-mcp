using AzureMcp.Commands.Postgres.Database;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.CommandLine.Parsing;
using Xunit;
using AzureMcp.Models.Command;
using System.Diagnostics;
using System.CommandLine;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AzureMcp.Tests.Commands.Postgres.Database;

[DebuggerStepThrough]
public class DatabaseQueryCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPostgresService _postgresService;
    private readonly ILogger<DatabaseQueryCommand> _logger;
    private readonly ITestOutputHelper _output;

    public DatabaseQueryCommandTests(ITestOutputHelper output)
    {
        _logger = Substitute.For<ILogger<DatabaseQueryCommand>>();
        _postgresService = Substitute.For<IPostgresService>();
        _output = output;

        var collection = new ServiceCollection();
        collection.AddSingleton(_postgresService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsQueryResults_WhenQueryIsValid()
    {
        var expectedResults = "[{\"id\":2}]";

        _postgresService.ExecuteQueryAsync("sub123", "rg1", "user1", "server1", "db123", "SELECT * FROM test;")
            .Returns(Task.FromResult(expectedResults));

        var command = new DatabaseQueryCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "sub123", "--resource-group", "rg1", "--user", "user1", "--server", "server1", "--database", "db123", "--query", "SELECT * FROM test;"]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<QueryResult>(json);
        Assert.NotNull(result);
        Assert.Equal(expectedResults, result.QueryResults);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenQueryFails()
    {
        var expectedResults = "";

        _postgresService.ExecuteQueryAsync("sub123", "rg1", "user1", "server1", "db123", "SELECT * FROM test;").Returns(expectedResults);

        var command = new DatabaseQueryCommand(_logger);
        var parser = new Parser(command.GetCommand());
        var args = parser.Parse(["--subscription", "sub123", "--resource-group", "rg1", "--user", "user1", "--server", "server1", "--database", "db123", "--query", "SELECT * FROM test;"]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<QueryResult>(json);
        Assert.NotNull(result);
        Assert.Equal(expectedResults, result.QueryResults);
    }

    private class QueryResult
    {
        [JsonPropertyName("QueryResult")]
        public required string QueryResults { get; set; }
    }

}