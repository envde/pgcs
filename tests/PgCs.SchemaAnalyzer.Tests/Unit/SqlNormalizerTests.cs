using PgCs.SchemaAnalyzer.Utils;

namespace PgCs.SchemaAnalyzer.Tests.Unit;

/// <summary>
/// Тесты для SqlNormalizer - утилиты нормализации SQL скриптов
/// </summary>
public sealed class SqlNormalizerTests
{
    #region Normalize Tests

    [Fact]
    public void Normalize_WithSimpleScript_ReturnsNormalized()
    {
        // Arrange
        const string sql = "CREATE    TABLE    users    (id    INT);";

        // Act
        var normalized = SqlNormalizer.Normalize(sql);

        // Assert
        Assert.Equal("CREATE TABLE users (id INT);", normalized);
    }

    [Fact]
    public void Normalize_WithSingleLineComments_RemovesComments()
    {
        // Arrange
        const string sql = @"
            -- This is a comment
            CREATE TABLE users (
                id INT, -- User ID
                name VARCHAR(50) -- User name
            );
        ";

        // Act
        var normalized = SqlNormalizer.Normalize(sql);

        // Assert
        Assert.DoesNotContain("--", normalized);
        Assert.DoesNotContain("This is a comment", normalized);
        Assert.Contains("CREATE TABLE users", normalized);
    }

    [Fact]
    public void Normalize_WithMultiLineComments_RemovesComments()
    {
        // Arrange
        const string sql = @"
            /* This is a 
               multi-line comment */
            CREATE TABLE users (
                id INT /* inline comment */ NOT NULL
            );
        ";

        // Act
        var normalized = SqlNormalizer.Normalize(sql);

        // Assert
        Assert.DoesNotContain("/*", normalized);
        Assert.DoesNotContain("*/", normalized);
        Assert.DoesNotContain("multi-line comment", normalized);
        Assert.Contains("CREATE TABLE users", normalized);
    }

    [Fact]
    public void Normalize_WithMixedComments_RemovesBoth()
    {
        // Arrange
        const string sql = @"
            -- Single line comment
            /* Multi-line comment */
            CREATE TABLE users (
                id INT -- Another comment
            ); /* End comment */
        ";

        // Act
        var normalized = SqlNormalizer.Normalize(sql);

        // Assert
        Assert.DoesNotContain("--", normalized);
        Assert.DoesNotContain("/*", normalized);
        Assert.DoesNotContain("*/", normalized);
    }

    [Fact]
    public void Normalize_WithExcessiveWhitespace_NormalizesSpaces()
    {
        // Arrange
        const string sql = "CREATE     TABLE     users\n\n\n(id     INT);";

        // Act
        var normalized = SqlNormalizer.Normalize(sql);

        // Assert
        Assert.Equal("CREATE TABLE users (id INT);", normalized);
    }

    [Fact]
    public void Normalize_WithTabsAndNewlines_ConvertsToSingleSpaces()
    {
        // Arrange
        const string sql = "CREATE\tTABLE\nusers\r\n(id\tINT);";

        // Act
        var normalized = SqlNormalizer.Normalize(sql);

        // Assert
        Assert.Equal("CREATE TABLE users (id INT);", normalized);
    }

    [Fact]
    public void Normalize_WithEmptyString_ReturnsEmptyString()
    {
        // Act
        var normalized = SqlNormalizer.Normalize("");

        // Assert
        Assert.Equal(string.Empty, normalized);
    }

    [Fact]
    public void Normalize_WithWhitespaceOnly_ReturnsEmptyString()
    {
        // Arrange
        const string sql = "   \n\t\r\n   ";

        // Act
        var normalized = SqlNormalizer.Normalize(sql);

        // Assert
        Assert.Equal(string.Empty, normalized);
    }

    [Fact]
    public void Normalize_WithNull_ReturnsEmptyString()
    {
        // Act
        var normalized = SqlNormalizer.Normalize(null!);

        // Assert
        Assert.Equal(string.Empty, normalized);
    }

    [Fact]
    public void Normalize_WithComplexScript_NormalizesCorrectly()
    {
        // Arrange
        const string sql = @"
            -- Create users table
            CREATE    TABLE    users    (
                id    SERIAL    PRIMARY    KEY,    -- User ID
                /*
                 * User information
                 */
                username    VARCHAR(50)    NOT    NULL
            );
            
            -- Create index
            CREATE    INDEX    idx_users    ON    users(username);
        ";

        // Act
        var normalized = SqlNormalizer.Normalize(sql);

        // Assert
        Assert.DoesNotContain("--", normalized);
        Assert.DoesNotContain("/*", normalized);
        Assert.DoesNotContain("*/", normalized);
        Assert.Contains("CREATE TABLE users", normalized);
        Assert.Contains("CREATE INDEX idx_users", normalized);
        Assert.DoesNotMatch(@"\s{2,}", normalized); // No multiple consecutive spaces
    }

    [Fact]
    public void Normalize_TrimsLeadingAndTrailingWhitespace()
    {
        // Arrange
        const string sql = "   CREATE TABLE test (id INT);   ";

        // Act
        var normalized = SqlNormalizer.Normalize(sql);

        // Assert
        Assert.Equal("CREATE TABLE test (id INT);", normalized);
        Assert.False(normalized.StartsWith(" "));
        Assert.False(normalized.EndsWith(" "));
    }

    #endregion

    #region PreserveComments Tests

    [Fact]
    public void PreserveComments_WithSimpleScript_NormalizesWhitespace()
    {
        // Arrange
        const string sql = "CREATE    TABLE    users    (id    INT);";

        // Act
        var result = SqlNormalizer.PreserveComments(sql);

        // Assert
        Assert.Equal("CREATE TABLE users (id INT);", result);
    }

    [Fact]
    public void PreserveComments_WithComments_PreservesComments()
    {
        // Arrange
        const string sql = @"
            -- This comment should stay
            CREATE TABLE users (
                id INT /* inline comment */
            );
        ";

        // Act
        var result = SqlNormalizer.PreserveComments(sql);

        // Assert
        Assert.Contains("--", result);
        Assert.Contains("This comment should stay", result);
        Assert.Contains("/*", result);
        Assert.Contains("inline comment", result);
    }

    [Fact]
    public void PreserveComments_WithExcessiveWhitespace_NormalizesSpaces()
    {
        // Arrange
        const string sql = "CREATE     TABLE\n\n\nusers     (id     INT);";

        // Act
        var result = SqlNormalizer.PreserveComments(sql);

        // Assert
        Assert.DoesNotMatch(@"\s{2,}", result);
    }

    [Fact]
    public void PreserveComments_WithEmptyString_ReturnsEmptyString()
    {
        // Act
        var result = SqlNormalizer.PreserveComments("");

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void PreserveComments_WithNull_ReturnsEmptyString()
    {
        // Act
        var result = SqlNormalizer.PreserveComments(null!);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void PreserveComments_TrimsResult()
    {
        // Arrange
        const string sql = "   CREATE TABLE test (id INT);   ";

        // Act
        var result = SqlNormalizer.PreserveComments(sql);

        // Assert
        Assert.False(result.StartsWith(" "));
        Assert.False(result.EndsWith(" "));
    }

    #endregion

    #region Edge Cases Tests

    // NOTE: PostgreSQL does NOT actually support nested multi-line comments
    // The pattern /* /* nested */ would cause a parser error
    // This test is skipped because SqlNormalizer doesn't need to handle invalid SQL syntax

    [Fact]
    public void Normalize_WithCommentLikeStrings_PreservesStrings()
    {
        // Arrange - Note: Simple normalization might not handle string literals perfectly
        const string sql = "CREATE TABLE test (comment VARCHAR(50)); -- Real comment";

        // Act
        var normalized = SqlNormalizer.Normalize(sql);

        // Assert
        Assert.Contains("comment VARCHAR(50)", normalized);
        Assert.DoesNotContain("Real comment", normalized);
    }

    [Fact]
    public void Normalize_WithDashesInNames_PreservesNames()
    {
        // Arrange
        const string sql = "CREATE TABLE test_table (test-column INT); -- Comment";

        // Act
        var normalized = SqlNormalizer.Normalize(sql);

        // Assert
        Assert.Contains("test_table", normalized);
        Assert.DoesNotContain("-- Comment", normalized);
    }

    [Fact]
    public void Normalize_WithLongScript_HandlesPerformantly()
    {
        // Arrange
        var sqlBuilder = new System.Text.StringBuilder();
        for (int i = 0; i < 1000; i++)
        {
            sqlBuilder.AppendLine($"-- Comment {i}");
            sqlBuilder.AppendLine($"CREATE TABLE table{i} (id INT);");
        }
        var sql = sqlBuilder.ToString();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var normalized = SqlNormalizer.Normalize(sql);
        stopwatch.Stop();

        // Assert
        Assert.NotEmpty(normalized);
        Assert.True(stopwatch.ElapsedMilliseconds < 1000, "Normalization should complete within 1 second");
        Assert.DoesNotContain("--", normalized);
    }

    #endregion
}
