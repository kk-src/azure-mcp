using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using AzureMcp.Commands.Postgres.Database;
using AzureMcp.Models.Command;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

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
        var expectedResults = new List<string> { "result1", "result2" };

        _postgresService.ExecuteQueryAsync("sub123", "rg1", "user1", "server1", "db123", "SELECT * FROM test;")
            .Returns(expectedResults);

        var command = new DatabaseQueryCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "sub123", "--resource-group", "rg1", "--user-name", "user1", "--server", "server1", "--database", "db123", "--query", "SELECT * FROM test;"]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<DatabaseQueryResult>(json);
        Assert.NotNull(result);
        Assert.Equal(expectedResults, result.QueryResult);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenQueryFails()
    {
        var expectedResults = new List<string>();

        _postgresService.ExecuteQueryAsync("sub123", "rg1", "user1", "server1", "db123", "SELECT * FROM test;")
            .Returns(expectedResults);

        var command = new DatabaseQueryCommand(_logger);
        var parser = new Parser(command.GetCommand());
        var args = parser.Parse(["--subscription", "sub123", "--resource-group", "rg1", "--user-name", "user1", "--server", "server1", "--database", "db123", "--query", "SELECT * FROM test;"]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.Null(response.Results);
    }

    [Theory]
    [InlineData("--subscription")]
    [InlineData("--resource-group")]
    [InlineData("--user-name")]
    [InlineData("--server")]
    [InlineData("--database")]
    [InlineData("--query")]
    public async Task ExecuteAsync_ReturnsError_WhenParameterIsMissing(string missingParameter)
    {
        var command = new DatabaseQueryCommand(_logger);
        var args = command.GetCommand().Parse(new string[]
        {
            missingParameter == "--subscription" ? "" : "--subscription", "sub123",
            missingParameter == "--resource-group" ? "" : "--resource-group", "rg1",
            missingParameter == "--user-name" ? "" : "--user-name", "user1",
            missingParameter == "--server" ? "" : "--server", "server123",
            missingParameter == "--database" ? "" : "--database", "db123",
            missingParameter == "--query" ? "" : "--query", "SELECT * FROM test;"
        });

        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(400, response.Status);
        Assert.Equal($"Missing required arguments: {missingParameter.TrimStart('-')}", response.Message);
    }

    private class DatabaseQueryResult
    {
        [JsonPropertyName("QueryResult")]
        public List<string> QueryResult { get; set; } = new List<string>();

    }

}
