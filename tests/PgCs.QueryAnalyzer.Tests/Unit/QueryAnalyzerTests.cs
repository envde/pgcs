namespace PgCs.QueryAnalyzer.Tests.Unit;

using Helpers;
using PgCs.Common.QueryAnalyzer.Models.Metadata;
using PgCs.Common.QueryAnalyzer.Models.Results;

public sealed class QueryAnalyzerTests
{
    private readonly QueryAnalyzer _sut = new();

    [Fact]
    public void AnalyzeQuery_WithSelectOneQuery_ReturnsCorrectMetadata()
    {
        // Arrange
        var sql = TestDataBuilder.BuildQuery(
            name: "GetUser",
            cardinality: "one",
            sqlBody: "SELECT id, username, email FROM users WHERE id = $1;"
        );

        // Act
        var result = _sut.AnalyzeQuery(sql);

        // Assert
        Assert.Equal("GetUser", result.MethodName);
        Assert.Equal(QueryType.Select, result.QueryType);
        Assert.Equal(ReturnCardinality.One, result.ReturnCardinality);
        Assert.Contains("SELECT id, username, email", result.SqlQuery);
        Assert.Single(result.Parameters);
        Assert.NotNull(result.ReturnType);
        Assert.Equal(3, result.ReturnType!.Columns.Count);
    }

    [Fact]
    public void AnalyzeQuery_WithInsertExecQuery_NoReturnType()
    {
        // Arrange
        var sql = TestDataBuilder.BuildQuery(
            name: "CreateUser",
            cardinality: "exec",
            sqlBody: "INSERT INTO users (username, email) VALUES ($1, $2);"
        );

        // Act
        var result = _sut.AnalyzeQuery(sql);

        // Assert
        Assert.Equal(QueryType.Insert, result.QueryType);
        Assert.Equal(ReturnCardinality.Exec, result.ReturnCardinality);
        Assert.Null(result.ReturnType);
        Assert.Equal(2, result.Parameters.Count);
    }

    [Fact]
    public void AnalyzeQuery_WithUpdateExecRowsQuery_HasReturnType()
    {
        // Arrange
        var sql = TestDataBuilder.BuildQuery(
            name: "UpdateUser",
            cardinality: "execrows",
            sqlBody: "UPDATE users SET email = @email::string WHERE id = @id::int RETURNING id, updated_at;"
        );

        // Act
        var result = _sut.AnalyzeQuery(sql);

        // Assert
        Assert.Equal(QueryType.Update, result.QueryType);
        Assert.Equal(ReturnCardinality.ExecRows, result.ReturnCardinality);
        Assert.NotNull(result.ReturnType);
        Assert.Equal(2, result.ReturnType!.Columns.Count);
    }

    [Fact]
    public void AnalyzeQuery_WithDeleteQuery_CorrectType()
    {
        // Arrange
        var sql = TestDataBuilder.BuildQuery(
            name: "DeleteUser",
            cardinality: "exec",
            sqlBody: "DELETE FROM users WHERE id = $1;"
        );

        // Act
        var result = _sut.AnalyzeQuery(sql);

        // Assert
        Assert.Equal(QueryType.Delete, result.QueryType);
    }

    [Fact]
    public void AnalyzeQuery_WithCteQuery_IsSelect()
    {
        // Arrange
        var sql = """
        -- name: GetStats :many
        WITH stats AS (
            SELECT COUNT(*) as total FROM users
        )
        SELECT total FROM stats;
        """;

        // Act
        var result = _sut.AnalyzeQuery(sql);

        // Assert
        Assert.Equal(QueryType.Select, result.QueryType);
        Assert.Equal(ReturnCardinality.Many, result.ReturnCardinality);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void AnalyzeQuery_WithInvalidInput_ThrowsArgumentException(string? input)
    {
    // Act & Assert
    Assert.ThrowsAny<ArgumentException>(() => _sut.AnalyzeQuery(input!));
    }

    [Fact]
    public void ExtractParameters_WithMixedSyntax_FindsAll()
    {
        // Arrange
        var sql = "SELECT * FROM users WHERE id = $id AND email = @email OR status = $status;";

        // Act
        var result = _sut.ExtractParameters(sql);

        // Assert
        Assert.Equal(3, result.Count);
        var names = result.Select(p => p.Name).OrderBy(n => n).ToList();
        var expectedNames = new[] { "email", "id", "status" }.OrderBy(n => n).ToList();
        Assert.Equal(expectedNames, names);
    }

    [Fact]
    public void ExtractParameters_WithDuplicates_Deduplicates()
    {
        // Arrange
        var sql = "SELECT * FROM t WHERE a = $param OR b = $param OR c = @param;";

        // Act
        var result = _sut.ExtractParameters(sql);

        // Assert
        Assert.Single(result);
        Assert.Equal("param", result.First().Name);
    }

    [Fact]
    public void InferReturnType_WithSelectStar_ReturnsVoid()
    {
        // Arrange
        var sql = "SELECT * FROM users;";

        // Act
        var result = _sut.InferReturnType(sql, "TestMethod");

        // Assert
        Assert.Equal("void", result.ModelName);
        Assert.Empty(result.Columns);
        Assert.False(result.RequiresCustomModel);
    }

    [Fact]
    public void InferReturnType_WithNamedColumns_GeneratesModel()
    {
        // Arrange
        var sql = "SELECT id, username, email FROM users;";

        // Act
        var result = _sut.InferReturnType(sql, "GetUser");

        // Assert
        Assert.Equal("GetUserResult", result.ModelName);
        Assert.Equal(3, result.Columns.Count);
        Assert.True(result.RequiresCustomModel);
    }

    [Fact]
    public void ParseAnnotations_ValidFormat_ParsesCorrectly()
    {
        // Arrange
        var comments = "-- name: GetUser :one";

        // Act
        var result = _sut.ParseAnnotations(comments);

        // Assert
        Assert.Equal("GetUser", result.Name);
        Assert.Equal(ReturnCardinality.One, result.Cardinality);
    }

    [Theory]
    [InlineData("-- name: Query :one", "Query", ReturnCardinality.One)]
    [InlineData("-- name: List :many", "List", ReturnCardinality.Many)]
    [InlineData("-- name: Execute :exec", "Execute", ReturnCardinality.Exec)]
    [InlineData("-- name: Update :execrows", "Update", ReturnCardinality.ExecRows)]
    public void ParseAnnotations_AllCardinalities_ParsesCorrectly(
        string comment, string expectedName, ReturnCardinality expectedCardinality)
    {
        // Act
        var result = _sut.ParseAnnotations(comment);

        // Assert
        Assert.Equal(expectedName, result.Name);
        Assert.Equal(expectedCardinality, result.Cardinality);
    }

    [Fact]
    public async Task AnalyzeFileAsync_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var nonExistentPath = "/tmp/nonexistent_file_12345.sql";

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(async () => await _sut.AnalyzeFileAsync(nonExistentPath));
    }

    [Fact]
    public async Task AnalyzeFileAsync_WithMultipleQueries_ParsesAll()
    {
        // Arrange
        using var tempFile = TestFileHelper.CreateTempFile("""
        -- name: Q1 :one
        SELECT id FROM users WHERE id = $1;
        
        -- name: Q2 :many
        SELECT * FROM orders;
        
        -- name: Q3 :exec
        DELETE FROM logs WHERE created_at < $1;
        """);

        // Act
        var result = (await _sut.AnalyzeFileAsync(tempFile.Path)).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        var methodNames = result.Select(q => q.MethodName).OrderBy(n => n).ToList();
        var expectedMethodNames = new[] { "Q1", "Q2", "Q3" }.OrderBy(n => n).ToList();
        Assert.Equal(expectedMethodNames, methodNames);
    }

    [Fact]
    public async Task AnalyzeFileAsync_SkipsQueriesWithoutAnnotations()
    {
        // Arrange
        using var tempFile = TestFileHelper.CreateTempFile("""
        -- name: Valid :one
        SELECT id FROM users;
        
        -- This is just a comment
        SELECT * FROM orders;
        
        -- name: AlsoValid :many
        SELECT * FROM logs;
        """);

        // Act
        var result = (await _sut.AnalyzeFileAsync(tempFile.Path)).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        var methodNames = result.Select(q => q.MethodName).OrderBy(n => n).ToList();
        var expectedMethodNames = new[] { "AlsoValid", "Valid" }.OrderBy(n => n).ToList();
        Assert.Equal(expectedMethodNames, methodNames);
    }
}