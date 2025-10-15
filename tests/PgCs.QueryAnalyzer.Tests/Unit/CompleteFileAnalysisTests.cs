using PgCs.Common.QueryAnalyzer.Models.Enums;
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
        var result = await _sut.AnalyzeFileAsync(filePath);

        // Assert
        result.Should().NotBeEmpty("файл содержит множество запросов с аннотациями");
        result.Should().HaveCountGreaterThan(30, "в Queries.sql более 30 запросов");
    }

    [Fact]
    public async Task AnalyzeFileAsync_QueriesSql_AllQueriesHaveMethodNames()
    {
        // Arrange
        var filePath = TestFileHelper.GetTestDataPath("Queries.sql");

        // Act
        var result = await _sut.AnalyzeFileAsync(filePath);

        // Assert
        result.Should().OnlyContain(q => !string.IsNullOrWhiteSpace(q.MethodName));
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
        var result = await _sut.AnalyzeFileAsync(filePath);

        // Assert
        var methodNames = result.Select(q => q.MethodName).ToList();
        methodNames.Should().Contain(expectedQueries);
    }

    [Fact]
    public async Task AnalyzeFileAsync_QueriesSql_QueryTypes_AreCorrect()
    {
        // Arrange
        var filePath = TestFileHelper.GetTestDataPath("Queries.sql");

        // Act
        var result = await _sut.AnalyzeFileAsync(filePath);

        // Assert
        using var _ = new AssertionScope();
        
        var selectQueries = result.Where(q => q.QueryType == QueryType.Select).ToList();
        var insertQueries = result.Where(q => q.QueryType == QueryType.Insert).ToList();
        var updateQueries = result.Where(q => q.QueryType == QueryType.Update).ToList();
        var deleteQueries = result.Where(q => q.QueryType == QueryType.Delete).ToList();

        selectQueries.Should().NotBeEmpty("файл содержит SELECT запросы");
        insertQueries.Should().NotBeEmpty("файл содержит INSERT запросы");
        updateQueries.Should().NotBeEmpty("файл содержит UPDATE запросы");
        deleteQueries.Should().NotBeEmpty("файл содержит DELETE запросы");
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
        var result = await _sut.AnalyzeFileAsync(filePath);

        // Assert
        var query = result.FirstOrDefault(q => q.MethodName == methodName);
        query.Should().NotBeNull($"запрос {methodName} должен существовать");
        query.QueryType.Should().Be(expectedType);
        query.ReturnCardinality.Should().Be(expectedCardinality);
    }

    [Fact]
    public async Task AnalyzeFileAsync_QueriesSql_GetUserById_HasCorrectStructure()
    {
        // Arrange
        var filePath = TestFileHelper.GetTestDataPath("Queries.sql");

        // Act
        var result = await _sut.AnalyzeFileAsync(filePath);

        // Assert
        var query = result.FirstOrDefault(q => q.MethodName == "GetUserById");
        query.Should().NotBeNull();
        
        using var _ = new AssertionScope();
        query.MethodName.Should().Be("GetUserById");
        query.QueryType.Should().Be(QueryType.Select);
        query.ReturnCardinality.Should().Be(ReturnCardinality.One);
        query.Parameters.Should().HaveCount(1);
        query.Parameters.First().Position.Should().Be(1);
        query.ReturnType.Should().NotBeNull();
        query.ReturnType!.Columns.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AnalyzeFileAsync_QueriesSql_CreateUser_HasReturningClause()
    {
        // Arrange
        var filePath = TestFileHelper.GetTestDataPath("Queries.sql");

        // Act
        var result = await _sut.AnalyzeFileAsync(filePath);

        // Assert
        var query = result.FirstOrDefault(q => q.MethodName == "CreateUser");
        query.Should().NotBeNull();
        
        using var _ = new AssertionScope();
        query.ReturnCardinality.Should().Be(ReturnCardinality.One);
        query.ReturnType.Should().NotBeNull();
        query.ReturnType!.Columns.Should().Contain(c => c.Name == "id");
        query.Parameters.Should().HaveCountGreaterThan(3);
    }

    [Fact]
    public async Task AnalyzeFileAsync_QueriesSql_ListUsers_HasManyCardinality()
    {
        // Arrange
        var filePath = TestFileHelper.GetTestDataPath("Queries.sql");

        // Act
        var result = await _sut.AnalyzeFileAsync(filePath);

        // Assert
        var query = result.FirstOrDefault(q => q.MethodName == "ListUsers");
        query.Should().NotBeNull();
        query.ReturnCardinality.Should().Be(ReturnCardinality.Many);
        query.ReturnType.Should().NotBeNull();
    }

    [Fact]
    public async Task AnalyzeFileAsync_QueriesSql_ExecQueries_HaveNoReturnType()
    {
        // Arrange
        var filePath = TestFileHelper.GetTestDataPath("Queries.sql");

        // Act
        var result = await _sut.AnalyzeFileAsync(filePath);

        // Assert
        var execQueries = result.Where(q => q.ReturnCardinality == ReturnCardinality.Exec).ToList();
        execQueries.Should().NotBeEmpty();
        execQueries.Should().OnlyContain(q => q.ReturnType == null);
    }

    [Fact]
    public async Task AnalyzeFileAsync_QueriesSql_ExecRowsQueries_MayHaveReturnType()
    {
        // Arrange
        var filePath = TestFileHelper.GetTestDataPath("Queries.sql");

        // Act
        var result = await _sut.AnalyzeFileAsync(filePath);

        // Assert
        var execRowsQueries = result.Where(q => q.ReturnCardinality == ReturnCardinality.ExecRows).ToList();
        execRowsQueries.Should().NotBeEmpty();
        
        // Некоторые могут иметь RETURNING, некоторые нет
        execRowsQueries.Should().Contain(q => q.ReturnType != null, "некоторые ExecRows запросы имеют RETURNING");
    }

    [Fact]
    public async Task AnalyzeFileAsync_QueriesSql_CteQueries_RecognizedAsSelect()
    {
        // Arrange
        var filePath = TestFileHelper.GetTestDataPath("Queries.sql");

        // Act
        var result = await _sut.AnalyzeFileAsync(filePath);

        // Assert
        var cteQueries = result.Where(q => 
            q.SqlQuery.Contains("WITH", StringComparison.OrdinalIgnoreCase)).ToList();
        
        cteQueries.Should().NotBeEmpty("файл содержит CTE запросы");
        cteQueries.Should().OnlyContain(q => q.QueryType == QueryType.Select);
    }

    [Fact]
    public async Task AnalyzeFileAsync_QueriesSql_JoinQueries_ExtractColumnsCorrectly()
    {
        // Arrange
        var filePath = TestFileHelper.GetTestDataPath("Queries.sql");

        // Act
        var result = await _sut.AnalyzeFileAsync(filePath);

        // Assert
        var joinQueries = result.Where(q => 
            q.SqlQuery.Contains("JOIN", StringComparison.OrdinalIgnoreCase)).ToList();
        
        joinQueries.Should().NotBeEmpty("файл содержит JOIN запросы");
        joinQueries.Should().OnlyContain(q => q.ReturnType != null || q.ReturnCardinality == ReturnCardinality.Exec);
    }

    [Fact]
    public async Task AnalyzeFileAsync_QueriesSql_GetOrderWithUser_HasJoin()
    {
        // Arrange
        var filePath = TestFileHelper.GetTestDataPath("Queries.sql");

        // Act
        var result = await _sut.AnalyzeFileAsync(filePath);

        // Assert
        var query = result.FirstOrDefault(q => q.MethodName == "GetOrderWithUser");
        query.Should().NotBeNull();
        query.SqlQuery.Should().Contain("JOIN");
        query.ReturnType.Should().NotBeNull();
    }

    [Fact]
    public async Task AnalyzeFileAsync_QueriesSql_WindowFunctionQueries_Recognized()
    {
        // Arrange
        var filePath = TestFileHelper.GetTestDataPath("Queries.sql");

        // Act
        var result = await _sut.AnalyzeFileAsync(filePath);

        // Assert
        var windowQueries = result.Where(q => 
            q.SqlQuery.Contains("OVER (", StringComparison.OrdinalIgnoreCase)).ToList();
        
        windowQueries.Should().NotBeEmpty("файл содержит оконные функции");
        windowQueries.Should().OnlyContain(q => q.QueryType == QueryType.Select);
    }

    [Fact]
    public async Task AnalyzeFileAsync_QueriesSql_ArrayOperators_Recognized()
    {
        // Arrange
        var filePath = TestFileHelper.GetTestDataPath("Queries.sql");

        // Act
        var result = await _sut.AnalyzeFileAsync(filePath);

        // Assert
        var arrayQueries = result.Where(q => 
            q.SqlQuery.Contains("ANY (", StringComparison.OrdinalIgnoreCase) ||
            q.SqlQuery.Contains("@>", StringComparison.OrdinalIgnoreCase)).ToList();
        
        arrayQueries.Should().NotBeEmpty("файл содержит операции с массивами");
    }

    [Fact]
    public async Task AnalyzeFileAsync_QueriesSql_JsonbOperators_Recognized()
    {
        // Arrange
        var filePath = TestFileHelper.GetTestDataPath("Queries.sql");

        // Act
        var result = await _sut.AnalyzeFileAsync(filePath);

        // Assert
        var jsonbQueries = result.Where(q => 
            q.SqlQuery.Contains("jsonb", StringComparison.OrdinalIgnoreCase) ||
            q.SqlQuery.Contains("->", StringComparison.OrdinalIgnoreCase)).ToList();
        
        jsonbQueries.Should().NotBeEmpty("файл содержит операции с JSONB");
    }

    [Fact]
    public async Task AnalyzeFileAsync_QueriesSql_AllQueriesHaveValidSql()
    {
        // Arrange
        var filePath = TestFileHelper.GetTestDataPath("Queries.sql");

        // Act
        var result = await _sut.AnalyzeFileAsync(filePath);

        // Assert
        result.Should().OnlyContain(q => !string.IsNullOrWhiteSpace(q.SqlQuery));
        result.Should().OnlyContain(q => q.SqlQuery.Length > 10, "SQL не должен быть слишком коротким");
    }

    [Fact]
    public async Task AnalyzeFileAsync_QueriesSql_NoQueryHasDuplicateMethodName()
    {
        // Arrange
        var filePath = TestFileHelper.GetTestDataPath("Queries.sql");

        // Act
        var result = await _sut.AnalyzeFileAsync(filePath);

        // Assert
        var methodNames = result.Select(q => q.MethodName).ToList();
        methodNames.Should().OnlyHaveUniqueItems("имена методов должны быть уникальными");
    }

    [Fact]
    public async Task AnalyzeFileAsync_QueriesSql_ParameterPositions_AreSequential()
    {
        // Arrange
        var filePath = TestFileHelper.GetTestDataPath("Queries.sql");

        // Act
        var result = await _sut.AnalyzeFileAsync(filePath);

        // Assert
        foreach (var query in result.Where(q => q.Parameters.Count > 0))
        {
            var positions = query.Parameters.Select(p => p.Position).OrderBy(p => p).ToList();
            positions.Should().BeInAscendingOrder();
            positions.First().Should().Be(1, $"первый параметр в {query.MethodName} должен иметь позицию 1");
        }
    }

    [Fact]
    public async Task AnalyzeFileAsync_QueriesSql_SelectQueries_HaveReturnTypes()
    {
        // Arrange
        var filePath = TestFileHelper.GetTestDataPath("Queries.sql");

        // Act
        var result = await _sut.AnalyzeFileAsync(filePath);

        // Assert
        var selectQueries = result.Where(q => 
            q.QueryType == QueryType.Select && 
            q.ReturnCardinality != ReturnCardinality.Exec).ToList();
        
        selectQueries.Should().NotBeEmpty();
        
        // Должны иметь ReturnType (кроме SELECT *)
        var queriesWithReturnType = selectQueries.Where(q => q.ReturnType != null).ToList();
        queriesWithReturnType.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AnalyzeFileAsync_QueriesSql_ModelNames_AreGenerated()
    {
        // Arrange
        var filePath = TestFileHelper.GetTestDataPath("Queries.sql");

        // Act
        var result = await _sut.AnalyzeFileAsync(filePath);

        // Assert
        var queriesWithReturnType = result.Where(q => q.ReturnType != null).ToList();
        queriesWithReturnType.Should().NotBeEmpty();
        queriesWithReturnType.Should().OnlyContain(q => !string.IsNullOrWhiteSpace(q.ReturnType!.ModelName));
    }

    [Fact]
    public async Task AnalyzeFileAsync_QueriesSql_Statistics_AreCorrect()
    {
        // Arrange
        var filePath = TestFileHelper.GetTestDataPath("Queries.sql");

        // Act
        var result = await _sut.AnalyzeFileAsync(filePath);

        // Assert
        using var _ = new AssertionScope();
        
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
        _testOutputHelper.WriteLine($"=== Статистика анализа Queries.sql ===");
        _testOutputHelper.WriteLine($"Всего запросов: {statistics.Total}");
        _testOutputHelper.WriteLine($"SELECT: {statistics.Select}, INSERT: {statistics.Insert}, UPDATE: {statistics.Update}, DELETE: {statistics.Delete}");
        _testOutputHelper.WriteLine($":one: {statistics.One}, :many: {statistics.Many}, :exec: {statistics.Exec}, :execrows: {statistics.ExecRows}");
        _testOutputHelper.WriteLine($"С параметрами: {statistics.WithParameters}");
        _testOutputHelper.WriteLine($"С возвращаемым типом: {statistics.WithReturnType}");
        
        statistics.Total.Should().BeGreaterThan(30);
        statistics.Select.Should().BeGreaterThan(10);
        statistics.Insert.Should().BeGreaterThan(3);
        statistics.Update.Should().BeGreaterThan(3);
    }
}