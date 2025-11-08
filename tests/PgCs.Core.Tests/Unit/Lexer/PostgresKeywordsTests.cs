using PgCs.Core.Lexer;

namespace PgCs.Core.Tests.Unit.Lexer;

/// <summary>
/// Comprehensive tests for PostgresKeywords static class
/// Tests keyword recognition for all PostgreSQL keywords
/// </summary>
public sealed class PostgresKeywordsTests
{
    [Theory]
    [InlineData("SELECT", true)]
    [InlineData("INSERT", true)]
    [InlineData("UPDATE", true)]
    [InlineData("DELETE", true)]
    [InlineData("CREATE", true)]
    [InlineData("DROP", true)]
    [InlineData("ALTER", true)]
    [InlineData("TABLE", true)]
    [InlineData("WHERE", true)]
    [InlineData("FROM", true)]
    [InlineData("JOIN", true)]
    [InlineData("ON", true)]
    [InlineData("notakeyword", false)]
    [InlineData("user_id", false)]
    [InlineData("my_table", false)]
    public void IsKeyword_WithStringParameter_ReturnsExpectedResult(string word, bool expected)
    {
        // Act
        var result = PostgresKeywords.IsKeyword(word);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("select", true)]
    [InlineData("SELECT", true)]
    [InlineData("SeLeCt", true)]
    [InlineData("insert", true)]
    [InlineData("INSERT", true)]
    [InlineData("create", true)]
    [InlineData("CREATE", true)]
    public void IsKeyword_IsCaseInsensitive(string word, bool expected)
    {
        // Act
        var result = PostgresKeywords.IsKeyword(word);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsKeyword_WithSpan_AllDDLKeywords_Recognized()
    {
        // Arrange
        var ddlKeywords = new[] { "CREATE", "DROP", "ALTER", "TRUNCATE", "COMMENT" };

        // Act & Assert
        foreach (var keyword in ddlKeywords)
        {
            Assert.True(PostgresKeywords.IsKeyword(keyword.AsSpan()), $"DDL keyword '{keyword}' should be recognized");
        }
    }

    [Fact]
    public void IsKeyword_WithSpan_AllDMLKeywords_Recognized()
    {
        // Arrange
        var dmlKeywords = new[] { "SELECT", "INSERT", "UPDATE", "DELETE", "MERGE" };

        // Act & Assert
        foreach (var keyword in dmlKeywords)
        {
            Assert.True(PostgresKeywords.IsKeyword(keyword.AsSpan()), $"DML keyword '{keyword}' should be recognized");
        }
    }

    [Fact]
    public void IsKeyword_AllObjectTypes_Recognized()
    {
        // Arrange
        var objectTypes = new[]
        {
            "TABLE", "VIEW", "INDEX", "SEQUENCE", "SCHEMA", "DATABASE",
            "FUNCTION", "PROCEDURE", "TRIGGER", "TYPE", "DOMAIN", "ENUM"
        };

        // Act & Assert
        foreach (var type in objectTypes)
        {
            Assert.True(PostgresKeywords.IsKeyword(type), $"Object type '{type}' should be recognized");
        }
    }

    [Fact]
    public void IsKeyword_AllConstraintKeywords_Recognized()
    {
        // Arrange
        var constraintKeywords = new[]
        {
            "CONSTRAINT", "PRIMARY", "FOREIGN", "UNIQUE", "CHECK",
            "DEFAULT", "REFERENCES", "KEY"
        };

        // Act & Assert
        foreach (var keyword in constraintKeywords)
        {
            Assert.True(PostgresKeywords.IsKeyword(keyword), $"Constraint keyword '{keyword}' should be recognized");
        }
    }

    [Fact]
    public void IsKeyword_AllJoinKeywords_Recognized()
    {
        // Arrange
        var joinKeywords = new[]
        {
            "JOIN", "LEFT", "RIGHT", "INNER", "OUTER", "CROSS",
            "ON", "USING"
        };

        // Act & Assert
        foreach (var keyword in joinKeywords)
        {
            Assert.True(PostgresKeywords.IsKeyword(keyword), $"Join keyword '{keyword}' should be recognized");
        }
    }

    [Fact]
    public void IsKeyword_AllSetOperations_Recognized()
    {
        // Arrange
        var setOps = new[] { "UNION", "INTERSECT", "EXCEPT", "DISTINCT", "ALL" };

        // Act & Assert
        foreach (var op in setOps)
        {
            Assert.True(PostgresKeywords.IsKeyword(op), $"Set operation '{op}' should be recognized");
        }
    }

    [Fact]
    public void IsKeyword_AllNumericTypes_Recognized()
    {
        // Arrange
        var numericTypes = new[]
        {
            "INTEGER", "INT", "BIGINT", "SMALLINT", "SERIAL", "BIGSERIAL",
            "NUMERIC", "DECIMAL", "REAL", "DOUBLE", "FLOAT"
        };

        // Act & Assert
        foreach (var type in numericTypes)
        {
            Assert.True(PostgresKeywords.IsKeyword(type), $"Numeric type '{type}' should be recognized");
        }
    }

    [Fact]
    public void IsKeyword_AllStringTypes_Recognized()
    {
        // Arrange
        var stringTypes = new[] { "VARCHAR", "CHAR", "TEXT", "BYTEA" };

        // Act & Assert
        foreach (var type in stringTypes)
        {
            Assert.True(PostgresKeywords.IsKeyword(type), $"String type '{type}' should be recognized");
        }
    }

    [Fact]
    public void IsKeyword_AllDateTimeTypes_Recognized()
    {
        // Arrange
        var dateTimeTypes = new[]
        {
            "DATE", "TIME", "TIMESTAMP", "TIMESTAMPTZ", "INTERVAL"
        };

        // Act & Assert
        foreach (var type in dateTimeTypes)
        {
            Assert.True(PostgresKeywords.IsKeyword(type), $"DateTime type '{type}' should be recognized");
        }
    }

    [Fact]
    public void IsKeyword_AllBooleanTypes_Recognized()
    {
        // Arrange
        var boolTypes = new[] { "BOOLEAN", "BOOL" };

        // Act & Assert
        foreach (var type in boolTypes)
        {
            Assert.True(PostgresKeywords.IsKeyword(type), $"Boolean type '{type}' should be recognized");
        }
    }

    [Fact]
    public void IsKeyword_AllJSONTypes_Recognized()
    {
        // Arrange
        var jsonTypes = new[] { "JSON", "JSONB", "XML" };

        // Act & Assert
        foreach (var type in jsonTypes)
        {
            Assert.True(PostgresKeywords.IsKeyword(type), $"JSON/XML type '{type}' should be recognized");
        }
    }

    [Fact]
    public void IsKeyword_AllLogicalOperators_Recognized()
    {
        // Arrange
        var logicalOps = new[]
        {
            "AND", "OR", "NOT", "IN", "LIKE", "ILIKE", "SIMILAR",
            "BETWEEN", "IS", "ANY", "SOME"
        };

        // Act & Assert
        foreach (var op in logicalOps)
        {
            Assert.True(PostgresKeywords.IsKeyword(op), $"Logical operator '{op}' should be recognized");
        }
    }

    [Fact]
    public void IsKeyword_AllTransactionKeywords_Recognized()
    {
        // Arrange
        var transactionKeywords = new[]
        {
            "BEGIN", "COMMIT", "ROLLBACK", "SAVEPOINT", "TRANSACTION",
            "ISOLATION", "LEVEL", "READ", "WRITE", "ONLY"
        };

        // Act & Assert
        foreach (var keyword in transactionKeywords)
        {
            Assert.True(PostgresKeywords.IsKeyword(keyword), $"Transaction keyword '{keyword}' should be recognized");
        }
    }

    [Fact]
    public void IsKeyword_AllIndexMethods_Recognized()
    {
        // Arrange
        var indexMethods = new[]
        {
            "BTREE", "HASH", "GIST", "GIN", "SPGIST", "BRIN"
        };

        // Act & Assert
        foreach (var method in indexMethods)
        {
            Assert.True(PostgresKeywords.IsKeyword(method), $"Index method '{method}' should be recognized");
        }
    }

    [Fact]
    public void IsKeyword_AllWindowKeywords_Recognized()
    {
        // Arrange
        var windowKeywords = new[]
        {
            "WINDOW", "OVER", "ROWS", "GROUPS", "RANGE",
            "UNBOUNDED", "PRECEDING", "FOLLOWING", "CURRENT"
        };

        // Act & Assert
        foreach (var keyword in windowKeywords)
        {
            Assert.True(PostgresKeywords.IsKeyword(keyword), $"Window keyword '{keyword}' should be recognized");
        }
    }

    [Fact]
    public void IsKeyword_AllPartitionKeywords_Recognized()
    {
        // Arrange
        var partitionKeywords = new[]
        {
            "PARTITION", "RANGE", "LIST", "HASH", "VALUES",
            "MODULUS", "REMAINDER"
        };

        // Act & Assert
        foreach (var keyword in partitionKeywords)
        {
            Assert.True(PostgresKeywords.IsKeyword(keyword), $"Partition keyword '{keyword}' should be recognized");
        }
    }

    [Fact]
    public void IsKeyword_AllFunctionKeywords_Recognized()
    {
        // Arrange
        var functionKeywords = new[]
        {
            "RETURNS", "RETURN", "DECLARE", "LOOP", "EXIT", "CONTINUE",
            "RAISE", "NOTICE", "WARNING", "EXCEPTION", "INFO", "LOG", "DEBUG"
        };

        // Act & Assert
        foreach (var keyword in functionKeywords)
        {
            Assert.True(PostgresKeywords.IsKeyword(keyword), $"Function keyword '{keyword}' should be recognized");
        }
    }

    [Fact]
    public void IsKeyword_AllVolatilityKeywords_Recognized()
    {
        // Arrange
        var volatilityKeywords = new[]
        {
            "VOLATILE", "STABLE", "IMMUTABLE", "STRICT", "LEAKPROOF"
        };

        // Act & Assert
        foreach (var keyword in volatilityKeywords)
        {
            Assert.True(PostgresKeywords.IsKeyword(keyword), $"Volatility keyword '{keyword}' should be recognized");
        }
    }

    [Fact]
    public void IsKeyword_NonKeywordIdentifiers_NotRecognized()
    {
        // Arrange
        var nonKeywords = new[]
        {
            "user_id", "my_table", "column_name", "test123",
            "_private", "var$1", "customType"
        };

        // Act & Assert
        foreach (var word in nonKeywords)
        {
            Assert.False(PostgresKeywords.IsKeyword(word), $"'{word}' should not be recognized as keyword");
        }
    }

    [Fact]
    public void IsDataType_AllBasicTypes_Recognized()
    {
        // Arrange
        var basicTypes = new[]
        {
            "INTEGER", "INT", "INT4", "BIGINT", "INT8", "SMALLINT", "INT2",
            "NUMERIC", "DECIMAL", "REAL", "FLOAT4", "DOUBLE", "FLOAT8"
        };

        // Act & Assert
        foreach (var type in basicTypes)
        {
            Assert.True(PostgresKeywords.IsDataType(type.AsSpan()), $"Basic type '{type}' should be recognized");
        }
    }

    [Fact]
    public void IsDataType_AllSerialTypes_Recognized()
    {
        // Arrange
        var serialTypes = new[] { "SERIAL", "BIGSERIAL", "SMALLSERIAL" };

        // Act & Assert
        foreach (var type in serialTypes)
        {
            Assert.True(PostgresKeywords.IsDataType(type.AsSpan()), $"Serial type '{type}' should be recognized");
        }
    }

    [Fact]
    public void IsDataType_AllCharacterTypes_Recognized()
    {
        // Arrange
        var charTypes = new[]
        {
            "VARCHAR", "CHARACTER", "CHAR", "TEXT", "BYTEA", "NAME"
        };

        // Act & Assert
        foreach (var type in charTypes)
        {
            Assert.True(PostgresKeywords.IsDataType(type.AsSpan()), $"Character type '{type}' should be recognized");
        }
    }

    [Fact]
    public void IsDataType_AllTemporalTypes_Recognized()
    {
        // Arrange
        var temporalTypes = new[]
        {
            "DATE", "TIME", "TIMESTAMP", "TIMESTAMPTZ", "INTERVAL"
        };

        // Act & Assert
        foreach (var type in temporalTypes)
        {
            Assert.True(PostgresKeywords.IsDataType(type.AsSpan()), $"Temporal type '{type}' should be recognized");
        }
    }

    [Fact]
    public void IsDataType_AllNetworkTypes_Recognized()
    {
        // Arrange
        var networkTypes = new[] { "INET", "CIDR", "MACADDR", "MACADDR8" };

        // Act & Assert
        foreach (var type in networkTypes)
        {
            Assert.True(PostgresKeywords.IsDataType(type.AsSpan()), $"Network type '{type}' should be recognized");
        }
    }

    [Fact]
    public void IsDataType_AllGeometricTypes_Recognized()
    {
        // Arrange
        var geometricTypes = new[]
        {
            "POINT", "LINE", "LSEG", "BOX", "PATH", "POLYGON", "CIRCLE"
        };

        // Act & Assert
        foreach (var type in geometricTypes)
        {
            Assert.True(PostgresKeywords.IsDataType(type.AsSpan()), $"Geometric type '{type}' should be recognized");
        }
    }

    [Fact]
    public void IsDataType_AllTextSearchTypes_Recognized()
    {
        // Arrange
        var textSearchTypes = new[] { "TSQUERY", "TSVECTOR" };

        // Act & Assert
        foreach (var type in textSearchTypes)
        {
            Assert.True(PostgresKeywords.IsDataType(type.AsSpan()), $"Text search type '{type}' should be recognized");
        }
    }

    [Fact]
    public void IsDataType_AllOIDTypes_Recognized()
    {
        // Arrange
        var oidTypes = new[]
        {
            "OID", "REGPROC", "REGPROCEDURE", "REGOPER", "REGOPERATOR",
            "REGCLASS", "REGTYPE", "REGROLE", "REGNAMESPACE",
            "REGCONFIG", "REGDICTIONARY"
        };

        // Act & Assert
        foreach (var type in oidTypes)
        {
            Assert.True(PostgresKeywords.IsDataType(type.AsSpan()), $"OID type '{type}' should be recognized");
        }
    }

    [Fact]
    public void IsDataType_MiscTypes_Recognized()
    {
        // Arrange
        var miscTypes = new[]
        {
            "BOOLEAN", "BOOL", "UUID", "JSON", "JSONB", "XML",
            "ARRAY", "HSTORE", "MONEY", "BIT", "VARBIT"
        };

        // Act & Assert
        foreach (var type in miscTypes)
        {
            Assert.True(PostgresKeywords.IsDataType(type.AsSpan()), $"Misc type '{type}' should be recognized");
        }
    }

    [Fact]
    public void IsDataType_IsCaseInsensitive()
    {
        // Arrange
        var types = new[]
        {
            ("INTEGER", true), ("integer", true), ("InTeGeR", true),
            ("VARCHAR", true), ("varchar", true), ("VarChar", true),
            ("BOOLEAN", true), ("boolean", true), ("Boolean", true)
        };

        // Act & Assert
        foreach (var (type, expected) in types)
        {
            Assert.Equal(expected, PostgresKeywords.IsDataType(type.AsSpan()));
        }
    }

    [Fact]
    public void IsDataType_NonDataTypes_NotRecognized()
    {
        // Arrange
        var nonTypes = new[]
        {
            "SELECT", "FROM", "WHERE", "user_table", "my_column"
        };

        // Act & Assert
        foreach (var word in nonTypes)
        {
            Assert.False(PostgresKeywords.IsDataType(word.AsSpan()),
                $"'{word}' should not be recognized as data type");
        }
    }

    [Fact]
    public void IsKeyword_EmptyString_ReturnsFalse()
    {
        // Act
        var result = PostgresKeywords.IsKeyword("");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsDataType_EmptySpan_ReturnsFalse()
    {
        // Act
        var result = PostgresKeywords.IsDataType(ReadOnlySpan<char>.Empty);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsKeyword_WithSpan_MatchesStringVersion()
    {
        // Arrange
        var keywords = new[]
        {
            "SELECT", "INSERT", "UPDATE", "DELETE", "CREATE", "DROP"
        };

        // Act & Assert
        foreach (var keyword in keywords)
        {
            var stringResult = PostgresKeywords.IsKeyword(keyword);
            var spanResult = PostgresKeywords.IsKeyword(keyword.AsSpan());

            Assert.Equal(stringResult, spanResult);
        }
    }
}
