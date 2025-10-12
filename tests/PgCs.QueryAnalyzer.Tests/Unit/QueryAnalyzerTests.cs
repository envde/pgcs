namespace PgCs.QueryAnalyzer.Tests.Unit;

using Helpers;

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
        using var _ = new AssertionScope();
        result.MethodName.Should().Be("GetUser");
        result.QueryType.Should().Be(QueryType.Select);
        result.ReturnCardinality.Should().Be(ReturnCardinality.One);
        result.SqlQuery.Should().Contain("SELECT id, username, email");
        result.Parameters.Should().HaveCount(1);
        result.ReturnType.Should().NotBeNull();
        result.ReturnType!.Columns.Should().HaveCount(3);
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
        result.QueryType.Should().Be(QueryType.Insert);
        result.ReturnCardinality.Should().Be(ReturnCardinality.Exec);
        result.ReturnType.Should().BeNull();
        result.Parameters.Should().HaveCount(2);
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
        result.QueryType.Should().Be(QueryType.Update);
        result.ReturnCardinality.Should().Be(ReturnCardinality.ExecRows);
        result.ReturnType.Should().NotBeNull();
        result.ReturnType!.Columns.Should().HaveCount(2);
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
        result.QueryType.Should().Be(QueryType.Delete);
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
        result.QueryType.Should().Be(QueryType.Select);
        result.ReturnCardinality.Should().Be(ReturnCardinality.Many);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void AnalyzeQuery_WithInvalidInput_ThrowsArgumentException(string? input)
    {
        // Act
        var act = () => _sut.AnalyzeQuery(input!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ExtractParameters_WithMixedSyntax_FindsAll()
    {
        // Arrange
        var sql = "SELECT * FROM users WHERE id = $id AND email = @email OR status = $status;";

        // Act
        var result = _sut.ExtractParameters(sql);

        // Assert
        result.Should().HaveCount(3);
        result.Select(p => p.Name).Should().BeEquivalentTo(["id", "email", "status"]);
    }

    [Fact]
    public void ExtractParameters_WithDuplicates_Deduplicates()
    {
        // Arrange
        var sql = "SELECT * FROM t WHERE a = $param OR b = $param OR c = @param;";

        // Act
        var result = _sut.ExtractParameters(sql);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("param");
    }

    [Fact]
    public void InferReturnType_WithSelectStar_ReturnsVoid()
    {
        // Arrange
        var sql = "SELECT * FROM users;";

        // Act
        var result = _sut.InferReturnType(sql);

        // Assert
        result.ModelName.Should().Be("void");
        result.Columns.Should().BeEmpty();
        result.RequiresCustomModel.Should().BeFalse();
    }

    [Fact]
    public void InferReturnType_WithNamedColumns_GeneratesModel()
    {
        // Arrange
        var sql = "SELECT id, username, email FROM users;";

        // Act
        var result = _sut.InferReturnType(sql);

        // Assert
        result.ModelName.Should().NotBe("void");
        result.Columns.Should().HaveCount(3);
        result.RequiresCustomModel.Should().BeTrue();
    }

    [Fact]
    public void ParseAnnotations_ValidFormat_ParsesCorrectly()
    {
        // Arrange
        var comments = "-- name: GetUser :one";

        // Act
        var result = _sut.ParseAnnotations(comments);

        // Assert
        result.Name.Should().Be("GetUser");
        result.Cardinality.Should().Be(ReturnCardinality.One);
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
        result.Name.Should().Be(expectedName);
        result.Cardinality.Should().Be(expectedCardinality);
    }

    [Fact]
    public async Task AnalyzeFileAsync_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var nonExistentPath = "/tmp/nonexistent_file_12345.sql";

        // Act
        var act = async () => await _sut.AnalyzeFileAsync(nonExistentPath);

        // Assert
        await act.Should().ThrowAsync<FileNotFoundException>();
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
        var result = await _sut.AnalyzeFileAsync(tempFile.Path);

        // Assert
        result.Should().HaveCount(3);
        result.Select(q => q.MethodName).Should().BeEquivalentTo(["Q1", "Q2", "Q3"]);
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
        var result = await _sut.AnalyzeFileAsync(tempFile.Path);

        // Assert
        result.Should().HaveCount(2);
        result.Select(q => q.MethodName).Should().BeEquivalentTo(["Valid", "AlsoValid"]);
    }
}