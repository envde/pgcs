using PgCs.Common.QueryAnalyzer.Models.Metadata;
using PgCs.Common.QueryAnalyzer.Models.Results;
using PgCs.QueryAnalyzer.Tests.Helpers;
using Xunit.Abstractions;

namespace PgCs.QueryAnalyzer.Tests.Unit;

public class CompleteFileAnalysisTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly QueryAnalyzer _sut = new();

    public CompleteFileAnalysisTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task AnalyzeFileAsync_QueriesSql_ParsesAllAnnotatedQueries()
    {
        // Arrange
        var filePath = TestFileHelper.GetTestDataPath("Queries.sql");

        // Act
        var result = (await _sut.AnalyzeFileAsync(filePath)).ToList();

        // Assert
        Assert.True(result.Count > 0, "файл содержит множество запросов с аннотациями");
        Assert.True(result.Count > 30, "в Queries.sql более 30 запросов");
    }

    [Fact]
    public async Task AnalyzeFileAsync_QueriesSql_AllQueriesHaveMethodNames()
    {
        // Arrange
        var filePath = TestFileHelper.GetTestDataPath("Queries.sql");

        // Act
        var result = (await _sut.AnalyzeFileAsync(filePath)).ToList();

        // Assert
        Assert.All(result, q => Assert.False(string.IsNullOrWhiteSpace(q.MethodName)));
    }

    [Fact]
    public async Task AnalyzeFileAsync_QueriesSql_ContainsExpectedQueries()
    {
        // Arrange
        var filePath = TestFileHelper.GetTestDataPath("Queries.sql");
        var expectedQueries = new[]
        {
            "GetUserById",
            "GetUserByEmail",
            "ListUsers",
            "CreateUser",
            "UpdateUserStatus",
            "DeleteUser"
        };

        // Act
        var result = (await _sut.AnalyzeFileAsync(filePath)).ToList();

        // Assert
        var methodNames = result.Select(q => q.MethodName).ToList();
        Assert.All(expectedQueries, expected => Assert.Contains(expected, methodNames));
    }

    [Fact]
    public async Task AnalyzeFileAsync_QueriesSql_QueryTypes_AreCorrect()
    {
        // Arrange
        var filePath = TestFileHelper.GetTestDataPath("Queries.sql");

        // Act
        var result = (await _sut.AnalyzeFileAsync(filePath)).ToList();

        // Assert
        var selectQueries = result.Where(q => q.QueryType == QueryType.Select).ToList();
        var insertQueries = result.Where(q => q.QueryType == QueryType.Insert).ToList();
        var updateQueries = result.Where(q => q.QueryType == QueryType.Update).ToList();
        var deleteQueries = result.Where(q => q.QueryType == QueryType.Delete).ToList();

        Assert.True(selectQueries.Any(), "файл содержит SELECT запросы");
        Assert.True(insertQueries.Any(), "файл содержит INSERT запросы");
        Assert.True(updateQueries.Any(), "файл содержит UPDATE запросы");
        Assert.True(deleteQueries.Any(), "файл содержит DELETE запросы");
    }

    [Theory]
    [InlineData("GetUserById", QueryType.Select, ReturnCardinality.One)]
    [InlineData("ListUsers", QueryType.Select, ReturnCardinality.Many)]
    [InlineData("CreateUser", QueryType.Insert, ReturnCardinality.One)]
    [InlineData("CreateUserSimple", QueryType.Insert, ReturnCardinality.Exec)]
    [InlineData("UpdateUserStatus", QueryType.Update, ReturnCardinality.ExecRows)]
    [InlineData("DeleteUser", QueryType.Update, ReturnCardinality.ExecRows)] // Soft delete = UPDATE
    [InlineData("HardDeleteUser", QueryType.Delete, ReturnCardinality.ExecRows)]
    public async Task AnalyzeFileAsync_QueriesSql_SpecificQuery_HasCorrectMetadata(
        string methodName, QueryType expectedType, ReturnCardinality expectedCardinality)
    {
        // Arrange
        var filePath = TestFileHelper.GetTestDataPath("Queries.sql");

        // Act
        var result = (await _sut.AnalyzeFileAsync(filePath)).ToList();

        // Assert
        var query = result.FirstOrDefault(q => q.MethodName == methodName);
        Assert.NotNull(query);
        var nonNullQuery = query!;
        Assert.Equal(expectedType, nonNullQuery.QueryType);
        Assert.Equal(expectedCardinality, nonNullQuery.ReturnCardinality);
    }

    [Fact]
    public async Task AnalyzeFileAsync_QueriesSql_GetUserById_HasCorrectStructure()
    {
        // Arrange
        var filePath = TestFileHelper.GetTestDataPath("Queries.sql");

        // Act
        var result = (await _sut.AnalyzeFileAsync(filePath)).ToList();

        // Assert
        var query = result.FirstOrDefault(q => q.MethodName == "GetUserById");
        Assert.NotNull(query);
        var nonNullQuery = query!;
        Assert.Equal("GetUserById", nonNullQuery.MethodName);
        Assert.Equal(QueryType.Select, nonNullQuery.QueryType);
        Assert.Equal(ReturnCardinality.One, nonNullQuery.ReturnCardinality);
        Assert.Single(nonNullQuery.Parameters);
        Assert.Equal(1, nonNullQuery.Parameters[0].Position);
        var returnType = nonNullQuery.ReturnType;
        Assert.NotNull(returnType);
        Assert.NotEmpty(returnType!.Columns);
    }

    [Fact]
    public async Task AnalyzeFileAsync_QueriesSql_CreateUser_HasReturningClause()
    {
        // Arrange
        var filePath = TestFileHelper.GetTestDataPath("Queries.sql");

        // Act
        var result = (await _sut.AnalyzeFileAsync(filePath)).ToList();

        // Assert
        var query = result.FirstOrDefault(q => q.MethodName == "CreateUser");
        Assert.NotNull(query);
        var nonNullQuery = query!;
        Assert.Equal(ReturnCardinality.One, nonNullQuery.ReturnCardinality);
        var returnType = nonNullQuery.ReturnType;
        Assert.NotNull(returnType);
        Assert.Contains(returnType!.Columns, c => c.Name == "id");
        Assert.True(nonNullQuery.Parameters.Count > 3);
    }

    [Fact]
    public async Task AnalyzeFileAsync_QueriesSql_ListUsers_HasManyCardinality()
    {
        // Arrange
        var filePath = TestFileHelper.GetTestDataPath("Queries.sql");

        // Act
        var result = (await _sut.AnalyzeFileAsync(filePath)).ToList();

        // Assert
        var query = result.FirstOrDefault(q => q.MethodName == "ListUsers");
        Assert.NotNull(query);
        var nonNullQuery = query!;
        Assert.Equal(ReturnCardinality.Many, nonNullQuery.ReturnCardinality);
        Assert.NotNull(nonNullQuery.ReturnType);
    }

    [Fact]
    public async Task AnalyzeFileAsync_QueriesSql_ExecQueries_HaveNoReturnType()
    {
        // Arrange
        var filePath = TestFileHelper.GetTestDataPath("Queries.sql");

        // Act
        var result = (await _sut.AnalyzeFileAsync(filePath)).ToList();

        // Assert
        var execQueries = result.Where(q => q.ReturnCardinality == ReturnCardinality.Exec).ToList();
        Assert.NotEmpty(execQueries);
        Assert.All(execQueries, q => Assert.Null(q.ReturnType));
    }

    [Fact]
    public async Task AnalyzeFileAsync_QueriesSql_ExecRowsQueries_MayHaveReturnType()
    {
        // Arrange
        var filePath = TestFileHelper.GetTestDataPath("Queries.sql");

        // Act
        var result = (await _sut.AnalyzeFileAsync(filePath)).ToList();

        // Assert
        var execRowsQueries = result.Where(q => q.ReturnCardinality == ReturnCardinality.ExecRows).ToList();
        Assert.NotEmpty(execRowsQueries);

        // Некоторые могут иметь RETURNING, некоторые нет
        Assert.True(execRowsQueries.Any(q => q.ReturnType != null), "некоторые ExecRows запросы имеют RETURNING");
    }

    [Fact]
    public async Task AnalyzeFileAsync_QueriesSql_CteQueries_RecognizedAsSelect()
    {
        // Arrange
        var filePath = TestFileHelper.GetTestDataPath("Queries.sql");

        // Act
        var result = (await _sut.AnalyzeFileAsync(filePath)).ToList();

        // Assert
        var cteQueries = result.Where(q =>
            q.SqlQuery.Contains("WITH", StringComparison.OrdinalIgnoreCase)).ToList();

        Assert.NotEmpty(cteQueries);
        Assert.All(cteQueries, q => Assert.Equal(QueryType.Select, q.QueryType));
    }

    [Fact]
    public async Task AnalyzeFileAsync_QueriesSql_JoinQueries_ExtractColumnsCorrectly()
    {
        // Arrange
        var filePath = TestFileHelper.GetTestDataPath("Queries.sql");

        // Act
        var result = (await _sut.AnalyzeFileAsync(filePath)).ToList();

        // Assert
        var joinQueries = result.Where(q =>
            q.SqlQuery.Contains("JOIN", StringComparison.OrdinalIgnoreCase)).ToList();

        Assert.NotEmpty(joinQueries);
        Assert.All(joinQueries, q => Assert.True(q.ReturnType != null || q.ReturnCardinality == ReturnCardinality.Exec));
    }

    [Fact]
    public async Task AnalyzeFileAsync_QueriesSql_GetOrderWithUser_HasJoin()
    {
        // Arrange
        var filePath = TestFileHelper.GetTestDataPath("Queries.sql");

        // Act
        var result = (await _sut.AnalyzeFileAsync(filePath)).ToList();

        // Assert
        var query = result.FirstOrDefault(q => q.MethodName == "GetOrderWithUser");
        Assert.NotNull(query);
        var nonNullQuery = query!;
        Assert.Contains("JOIN", nonNullQuery.SqlQuery, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(nonNullQuery.ReturnType);
    }

    [Fact]
    public async Task AnalyzeFileAsync_QueriesSql_WindowFunctionQueries_Recognized()
    {
        // Arrange
        var filePath = TestFileHelper.GetTestDataPath("Queries.sql");

        // Act
        var result = (await _sut.AnalyzeFileAsync(filePath)).ToList();

        // Assert
        var windowQueries = result.Where(q =>
            q.SqlQuery.Contains("OVER (", StringComparison.OrdinalIgnoreCase)).ToList();

        Assert.NotEmpty(windowQueries);
        Assert.All(windowQueries, q => Assert.Equal(QueryType.Select, q.QueryType));
    }

    [Fact]
    public async Task AnalyzeFileAsync_QueriesSql_ArrayOperators_Recognized()
    {
        // Arrange
        var filePath = TestFileHelper.GetTestDataPath("Queries.sql");

        // Act
        var result = (await _sut.AnalyzeFileAsync(filePath)).ToList();

        // Assert
        var arrayQueries = result.Where(q =>
            q.SqlQuery.Contains("ANY (", StringComparison.OrdinalIgnoreCase) ||
            q.SqlQuery.Contains("@>", StringComparison.OrdinalIgnoreCase)).ToList();

        Assert.NotEmpty(arrayQueries);
    }

    [Fact]
    public async Task AnalyzeFileAsync_QueriesSql_JsonbOperators_Recognized()
    {
        // Arrange
        var filePath = TestFileHelper.GetTestDataPath("Queries.sql");

        // Act
        var result = (await _sut.AnalyzeFileAsync(filePath)).ToList();

        // Assert
        var jsonbQueries = result.Where(q =>
            q.SqlQuery.Contains("jsonb", StringComparison.OrdinalIgnoreCase) ||
            q.SqlQuery.Contains("->", StringComparison.OrdinalIgnoreCase)).ToList();

        Assert.NotEmpty(jsonbQueries);
    }

    [Fact]
    public async Task AnalyzeFileAsync_QueriesSql_AllQueriesHaveValidSql()
    {
        // Arrange
        var filePath = TestFileHelper.GetTestDataPath("Queries.sql");

        // Act
        var result = (await _sut.AnalyzeFileAsync(filePath)).ToList();

        // Assert
        Assert.All(result, q => Assert.False(string.IsNullOrWhiteSpace(q.SqlQuery)));
        Assert.True(result.All(q => q.SqlQuery.Length > 10), "SQL не должен быть слишком коротким");
    }

    [Fact]
    public async Task AnalyzeFileAsync_QueriesSql_NoQueryHasDuplicateMethodName()
    {
        // Arrange
        var filePath = TestFileHelper.GetTestDataPath("Queries.sql");

        // Act
        var result = (await _sut.AnalyzeFileAsync(filePath)).ToList();

        // Assert
        var methodNames = result.Select(q => q.MethodName).ToList();
        Assert.True(methodNames.Distinct().Count() == methodNames.Count, "имена методов должны быть уникальными");
    }

    [Fact]
    public async Task AnalyzeFileAsync_QueriesSql_ParameterPositions_AreSequential()
    {
        // Arrange
        var filePath = TestFileHelper.GetTestDataPath("Queries.sql");

        // Act
        var result = (await _sut.AnalyzeFileAsync(filePath)).ToList();

        // Assert
        foreach (var query in result.Where(q => q.Parameters.Count > 0))
        {
            var positions = query.Parameters.Select(p => p.Position).ToList();
            var orderedPositions = positions.OrderBy(p => p).ToList();

            Assert.Equal(orderedPositions, positions);
            Assert.True(orderedPositions.First() == 1, $"первый параметр в {query.MethodName} должен иметь позицию 1");
        }
    }

    [Fact]
    public async Task AnalyzeFileAsync_QueriesSql_SelectQueries_HaveReturnTypes()
    {
        // Arrange
        var filePath = TestFileHelper.GetTestDataPath("Queries.sql");

        // Act
        var result = (await _sut.AnalyzeFileAsync(filePath)).ToList();

        // Assert
        var selectQueries = result.Where(q =>
            q.QueryType == QueryType.Select &&
            q.ReturnCardinality != ReturnCardinality.Exec).ToList();

        Assert.NotEmpty(selectQueries);

        // Должны иметь ReturnType (кроме SELECT *)
        var queriesWithReturnType = selectQueries.Where(q => q.ReturnType != null).ToList();
        Assert.NotEmpty(queriesWithReturnType);
    }

    [Fact]
    public async Task AnalyzeFileAsync_QueriesSql_ModelNames_AreGenerated()
    {
        // Arrange
        var filePath = TestFileHelper.GetTestDataPath("Queries.sql");

        // Act
        var result = (await _sut.AnalyzeFileAsync(filePath)).ToList();

        // Assert
        var queriesWithReturnType = result.Where(q => q.ReturnType != null).ToList();
        Assert.NotEmpty(queriesWithReturnType);
        Assert.All(queriesWithReturnType, q =>
        {
            var returnType = q.ReturnType;
            Assert.NotNull(returnType);
            Assert.False(string.IsNullOrWhiteSpace(returnType!.ModelName));
        });
    }

    [Fact]
    public async Task AnalyzeFileAsync_QueriesSql_Statistics_AreCorrect()
    {
        // Arrange
        var filePath = TestFileHelper.GetTestDataPath("Queries.sql");

        // Act
        var result = (await _sut.AnalyzeFileAsync(filePath)).ToList();

        // Assert
        var statistics = new
        {
            Total = result.Count,
            Select = result.Count(q => q.QueryType == QueryType.Select),
            Insert = result.Count(q => q.QueryType == QueryType.Insert),
            Update = result.Count(q => q.QueryType == QueryType.Update),
            Delete = result.Count(q => q.QueryType == QueryType.Delete),
            One = result.Count(q => q.ReturnCardinality == ReturnCardinality.One),
            Many = result.Count(q => q.ReturnCardinality == ReturnCardinality.Many),
            Exec = result.Count(q => q.ReturnCardinality == ReturnCardinality.Exec),
            ExecRows = result.Count(q => q.ReturnCardinality == ReturnCardinality.ExecRows),
            WithParameters = result.Count(q => q.Parameters.Count > 0),
            WithReturnType = result.Count(q => q.ReturnType != null)
        };

        // Выводим статистику для информации
        _testOutputHelper.WriteLine("=== Статистика анализа Queries.sql ===");
        _testOutputHelper.WriteLine($"Всего запросов: {statistics.Total}");
        _testOutputHelper.WriteLine($"SELECT: {statistics.Select}, INSERT: {statistics.Insert}, UPDATE: {statistics.Update}, DELETE: {statistics.Delete}");
        _testOutputHelper.WriteLine($":one: {statistics.One}, :many: {statistics.Many}, :exec: {statistics.Exec}, :execrows: {statistics.ExecRows}");
        _testOutputHelper.WriteLine($"С параметрами: {statistics.WithParameters}");
        _testOutputHelper.WriteLine($"С возвращаемым типом: {statistics.WithReturnType}");

        Assert.True(statistics.Total > 30);
        Assert.True(statistics.Select > 10);
        Assert.True(statistics.Insert > 3);
        Assert.True(statistics.Update > 3);
    }
}