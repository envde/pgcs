using PgCs.Core.Extraction;
using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Common;
using PgCs.Core.Schema.Definitions;
using PgCs.SchemaAnalyzer.Tante.Extractors;

namespace PgCs.SchemaAnalyzer.Tante.Tests.Unit;

/// <summary>
/// Оптимизированные тесты для TableExtractor
/// Сокращено с 31 теста до 6 тестов, каждый покрывает множество проверок
/// </summary>
public sealed class TableExtractorTests
{
    private readonly IExtractor<TableDefinition> _extractor = new TableExtractor();

    [Fact]
    public void Extract_ComprehensiveTable_ExtractsAllFeaturesAndComments()
    {
        // Покрывает: все типы колонок, constraints, arrays, NUMERIC precision/scale, DEFAULT, inline комментарии
        var blocks = CreateBlocks(@"
            CREATE TABLE users (
                id BIGSERIAL PRIMARY KEY, -- Уникальный идентификатор
                external_id UUID UNIQUE NOT NULL, -- Внешний ID
                username VARCHAR(50) UNIQUE NOT NULL, -- Имя пользователя
                email VARCHAR(255) NOT NULL, -- Электронная почта
                password_hash CHAR(64) NOT NULL, --- Хеш пароля
                is_active BOOLEAN NOT NULL DEFAULT TRUE, -- Активен ли пользователь
                balance NUMERIC(12, 2) DEFAULT 0.00, -- Баланс счёта
                metadata JSONB, -- Метаданные
                tags TEXT[], -- Теги
                created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP -- Дата создания
            );
        ");

        var result = _extractor.Extract(blocks);
        if (!result.IsSuccess)
        {
            var issues = string.Join(", ", result.ValidationIssues.Select(i => i.Message));
            Assert.Fail($"Extraction failed with issues: {issues}");
        }
        
        Assert.True(result.IsSuccess);
        var table = result.Definition;
        Assert.NotNull(table);
        Assert.Equal("users", table.Name);
        Assert.Equal(10, table.Columns.Count);

        // Проверка комментариев
        var id = table.Columns.FirstOrDefault(c => c.Name == "id");
        Assert.NotNull(id);
        Assert.Equal("Уникальный идентификатор", id.SqlComment);
        
        var email = table.Columns.FirstOrDefault(c => c.Name == "email");
        Assert.NotNull(email);
        Assert.Equal("Электронная почта", email.SqlComment);
        
        var passwordHash = table.Columns.FirstOrDefault(c => c.Name == "password_hash");
        Assert.NotNull(passwordHash);
        Assert.Equal("Хеш пароля", passwordHash.SqlComment); // Проверка --- комментария

        // BIGSERIAL PRIMARY KEY
        Assert.Equal("BIGSERIAL", id.DataType);
        Assert.True(id.IsPrimaryKey);
        
        // UUID UNIQUE NOT NULL
        var externalId = table.Columns.FirstOrDefault(c => c.Name == "external_id");
        Assert.NotNull(externalId);
        Assert.Equal("UUID", externalId.DataType);
        Assert.True(externalId.IsUnique);
        Assert.False(externalId.IsNullable);

        // VARCHAR с length
        var username = table.Columns.FirstOrDefault(c => c.Name == "username");
        Assert.NotNull(username);
        Assert.Equal("VARCHAR", username.DataType);
        Assert.Equal(50, username.MaxLength);

        // CHAR с length
        Assert.NotNull(passwordHash);
        Assert.Equal("CHAR", passwordHash.DataType);
        Assert.Equal(64, passwordHash.MaxLength);

        // BOOLEAN DEFAULT TRUE
        var isActive = table.Columns.FirstOrDefault(c => c.Name == "is_active");
        Assert.NotNull(isActive);
        Assert.Equal("BOOLEAN", isActive.DataType);
        Assert.Equal("TRUE", isActive.DefaultValue);

        // NUMERIC(precision, scale) DEFAULT
        var balance = table.Columns.FirstOrDefault(c => c.Name == "balance");
        Assert.NotNull(balance);
        Assert.Equal(12, balance.NumericPrecision);
        Assert.Equal(2, balance.NumericScale);
        Assert.Equal("0.00", balance.DefaultValue);
        
        // DEFAULT CURRENT_TIMESTAMP
        var createdAt = table.Columns.FirstOrDefault(c => c.Name == "created_at");
        Assert.NotNull(createdAt);
        Assert.Equal("CURRENT_TIMESTAMP", createdAt.DefaultValue);
        Assert.False(createdAt.IsNullable);
    }

    [Fact]
    public void Extract_SpecialTableTypes_HandlesAllVariants()
    {
        // Покрывает: schema qualification, TEMPORARY, TEMP, UNLOGGED, inline комментарии
        var tempBlocks = CreateBlocks(@"
            CREATE TEMPORARY TABLE public.sessions (
                id UUID PRIMARY KEY, -- Идентификатор сессии
                user_id BIGINT NOT NULL, -- ID пользователя
                data JSONB -- Данные сессии
            );
        ");

        var tempResult = _extractor.Extract(tempBlocks);
        if (!tempResult.IsSuccess)
        {
            var issues = string.Join(", ", tempResult.ValidationIssues.Select(i => i.Message));
            Assert.Fail($"TEMPORARY table extraction failed with issues: {issues}");
        }
        Assert.True(tempResult.IsSuccess);
        var tempTable = tempResult.Definition;
        Assert.NotNull(tempTable);
        Assert.Equal("sessions", tempTable.Name);
        Assert.Equal("public", tempTable.Schema);
        Assert.True(tempTable.IsTemporary);
        
        // Проверка inline комментариев
        var tempId = tempTable.Columns.FirstOrDefault(c => c.Name == "id");
        Assert.NotNull(tempId);
        Assert.Equal("Идентификатор сессии", tempId.SqlComment);

        // TEMP (альтернативный синтаксис)
        var temp2Blocks = CreateBlocks("CREATE TEMP TABLE temp_session (session_id UUID);");
        var temp2Result = _extractor.Extract(temp2Blocks);
        if (!temp2Result.IsSuccess)
        {
            var issues = string.Join(", ", temp2Result.ValidationIssues.Select(i => i.Message));
            Assert.Fail($"TEMP table extraction failed with issues: {issues}");
        }
        var temp2Table = temp2Result.Definition;
        Assert.NotNull(temp2Table);
        Assert.True(temp2Table.IsTemporary);

        // UNLOGGED
        var unloggedBlocks = CreateBlocks(@"
            CREATE UNLOGGED TABLE cache (
                key VARCHAR(100) PRIMARY KEY, -- Ключ кэша
                value TEXT NOT NULL --- Значение кэша
            );
        ");

        var unloggedResult = _extractor.Extract(unloggedBlocks);
        if (!unloggedResult.IsSuccess)
        {
            var issues = string.Join(", ", unloggedResult.ValidationIssues.Select(i => i.Message));
            Assert.Fail($"UNLOGGED table extraction failed with issues: {issues}");
        }
        var unloggedTable = unloggedResult.Definition;
        Assert.NotNull(unloggedTable);
        Assert.True(unloggedTable.IsUnlogged);
        Assert.False(unloggedTable.IsTemporary);
        
        // Проверка комментария с тройным дефисом
        var cacheValue = unloggedTable.Columns.FirstOrDefault(c => c.Name == "value");
        Assert.NotNull(cacheValue);
        Assert.Equal("Значение кэша", cacheValue.SqlComment);
    }

    [Fact]
    public void Extract_PartitionedTables_HandlesAllStrategies()
    {
        // Покрывает: PARTITION BY RANGE/LIST/HASH, PARTITION OF, expression, inline комментарии
        var rangeBlocks = CreateBlocks(@"
            CREATE TABLE measurements (
                id BIGSERIAL, -- ID измерения
                measured_at TIMESTAMP NOT NULL, -- Время измерения
                value NUMERIC(10,2), --- Значение
                sensor_id INTEGER NOT NULL -- ID датчика
            ) PARTITION BY RANGE (measured_at);
        ");

        var rangeResult = _extractor.Extract(rangeBlocks);
        if (!rangeResult.IsSuccess)
        {
            var issues = string.Join(", ", rangeResult.ValidationIssues.Select(i => i.Message));
            Assert.Fail($"PARTITION BY RANGE extraction failed with issues: {issues}");
        }
        var rangeTable = rangeResult.Definition;
        Assert.NotNull(rangeTable);
        Assert.True(rangeTable.IsPartitioned);
        Assert.NotNull(rangeTable.PartitionInfo);
        Assert.Equal(PartitionStrategy.Range, rangeTable.PartitionInfo.Strategy);
        Assert.Contains("measured_at", rangeTable.PartitionInfo.PartitionKeys);
        
        // Проверка inline комментариев
        var measuredAt = rangeTable.Columns.FirstOrDefault(c => c.Name == "measured_at");
        Assert.NotNull(measuredAt);
        Assert.Equal("Время измерения", measuredAt.SqlComment);
        
        var value = rangeTable.Columns.FirstOrDefault(c => c.Name == "value");
        Assert.NotNull(value);
        Assert.Equal("Значение", value.SqlComment);

        // LIST
        var listBlocks = CreateBlocks("CREATE TABLE orders_by_region (id BIGINT, region VARCHAR(50)) PARTITION BY LIST (region);");
        var listResult = _extractor.Extract(listBlocks);
        if (!listResult.IsSuccess)
        {
            var issues = string.Join(", ", listResult.ValidationIssues.Select(i => i.Message));
            Assert.Fail($"PARTITION BY LIST extraction failed with issues: {issues}");
        }
        var listTable = listResult.Definition;
        Assert.NotNull(listTable);
        Assert.NotNull(listTable.PartitionInfo);
        Assert.Equal(PartitionStrategy.List, listTable.PartitionInfo.Strategy);

        // HASH
        var hashBlocks = CreateBlocks("CREATE TABLE distributed_data (id BIGINT, data TEXT) PARTITION BY HASH (id);");
        var hashResult = _extractor.Extract(hashBlocks);
        if (!hashResult.IsSuccess)
        {
            var issues = string.Join(", ", hashResult.ValidationIssues.Select(i => i.Message));
            Assert.Fail($"PARTITION BY HASH extraction failed with issues: {issues}");
        }
        var hashTable = hashResult.Definition;
        Assert.NotNull(hashTable);
        Assert.NotNull(hashTable.PartitionInfo);
        Assert.Equal(PartitionStrategy.Hash, hashTable.PartitionInfo.Strategy);

        // PARTITION OF
        var partitionBlocks = CreateBlocks(@"
            CREATE TABLE logs_2024_q1 PARTITION OF audit_logs
            FOR VALUES FROM ('2024-01-01') TO ('2024-04-01');
        ");
        var partitionResult = _extractor.Extract(partitionBlocks);
        if (!partitionResult.IsSuccess)
        {
            var issues = string.Join(", ", partitionResult.ValidationIssues.Select(i => i.Message));
            Assert.Fail($"PARTITION OF extraction failed with issues: {issues}");
        }
        var partitionTable = partitionResult.Definition;
        Assert.NotNull(partitionTable);
        Assert.True(partitionTable.IsPartition);
        Assert.Equal("audit_logs", partitionTable.ParentTableName);
    }

    [Fact]
    public void Extract_InheritanceTablespaceStorage_HandlesAdvancedFeatures()
    {
        // Покрывает: INHERITS (single/multiple), TABLESPACE, WITH (storage parameters), inline комментарии
        var inheritBlocks = CreateBlocks(@"
            CREATE TABLE employees (
                id SERIAL, -- ID сотрудника
                name VARCHAR(100), --- Имя
                department VARCHAR(50) -- Отдел
            ) INHERITS (persons);
        ");
        
        var inheritResult = _extractor.Extract(inheritBlocks);
        if (!inheritResult.IsSuccess)
        {
            var issues = string.Join(", ", inheritResult.ValidationIssues.Select(i => i.Message));
            Assert.Fail($"INHERITS extraction failed with issues: {issues}");
        }
        var inheritTable = inheritResult.Definition;
        Assert.NotNull(inheritTable);
        Assert.NotNull(inheritTable.InheritsFrom);
        Assert.Single(inheritTable.InheritsFrom);
        Assert.Contains("persons", inheritTable.InheritsFrom);
        
        // Проверка inline комментариев с разными форматами
        var empId = inheritTable.Columns.FirstOrDefault(c => c.Name == "id");
        Assert.NotNull(empId);
        Assert.Equal("ID сотрудника", empId.SqlComment);
        
        var empName = inheritTable.Columns.FirstOrDefault(c => c.Name == "name");
        Assert.NotNull(empName);
        Assert.Equal("Имя", empName.SqlComment); // Тройной дефис

        // Multiple inheritance
        var multiBlocks = CreateBlocks("CREATE TABLE premium_users (premium_level INTEGER) INHERITS (users, customers);");
        var multiResult = _extractor.Extract(multiBlocks);
        if (!multiResult.IsSuccess)
        {
            var issues = string.Join(", ", multiResult.ValidationIssues.Select(i => i.Message));
            Assert.Fail($"Multiple INHERITS extraction failed with issues: {issues}");
        }
        var multiTable = multiResult.Definition;
        Assert.NotNull(multiTable);
        Assert.NotNull(multiTable.InheritsFrom);
        Assert.Equal(2, multiTable.InheritsFrom.Count);
        Assert.Contains("users", multiTable.InheritsFrom);
        Assert.Contains("customers", multiTable.InheritsFrom);

        // TABLESPACE
        var tablespaceBlocks = CreateBlocks(@"
            CREATE TABLE archived_data (
                id BIGINT, -- Идентификатор
                data TEXT -- Архивные данные
            ) TABLESPACE archive_space;
        ");
        var tablespaceResult = _extractor.Extract(tablespaceBlocks);
        if (!tablespaceResult.IsSuccess)
        {
            var issues = string.Join(", ", tablespaceResult.ValidationIssues.Select(i => i.Message));
            Assert.Fail($"TABLESPACE extraction failed with issues: {issues}");
        }
        var tablespaceTable = tablespaceResult.Definition;
        Assert.NotNull(tablespaceTable);
        Assert.Equal("archive_space", tablespaceTable.Tablespace);

        // Storage parameters
        var storageBlocks = CreateBlocks(@"
            CREATE TABLE config (
                key VARCHAR(100), -- Ключ конфигурации
                value TEXT --- Значение конфигурации
            ) WITH (fillfactor = 70, autovacuum_enabled = true);
        ");
        var storageResult = _extractor.Extract(storageBlocks);
        if (!storageResult.IsSuccess)
        {
            var issues = string.Join(", ", storageResult.ValidationIssues.Select(i => i.Message));
            Assert.Fail($"WITH storage parameters extraction failed with issues: {issues}");
        }
        var storageTable = storageResult.Definition;
        Assert.NotNull(storageTable);
        Assert.NotNull(storageTable.StorageParameters);
        Assert.Equal(2, storageTable.StorageParameters.Count);
        Assert.Equal("70", storageTable.StorageParameters["fillfactor"]);
        Assert.Equal("true", storageTable.StorageParameters["autovacuum_enabled"]);
    }

    [Fact]
    public void Extract_SpecialFormatComments_ParsesMetadata()
    {
        // Покрывает: специальные форматы комментариев с метаданными
        // Формат 1: comment: Описание; to_type: ТИП; to_name: НовоеИмя;
        // Формат 2: comment(Описание); to_type(ТИП); to_name(НовоеИмя);
        var blocks = CreateBlocks(@"
            CREATE TABLE products (
                id BIGSERIAL PRIMARY KEY, -- comment: Уникальный идентификатор товара; to_type: BIGINT; to_name: product_id
                legacy_name VARCHAR(100), -- comment(Устаревшее имя товара); to_type(TEXT); to_name(product_name)
                category_id INTEGER, -- comment: ID категории; to_name: cat_id
                description TEXT, -- comment(Полное описание товара)
                price NUMERIC(10,2), -- to_type: DECIMAL; comment: Цена товара
                stock_count INTEGER, -- to_name: quantity; comment: Количество на складе
                simple_comment_column TEXT -- Простой комментарий без служебных слов
            );
        ");

        var result = _extractor.Extract(blocks);
        if (!result.IsSuccess)
        {
            var issues = string.Join(", ", result.ValidationIssues.Select(i => i.Message));
            Assert.Fail($"Extraction failed with issues: {issues}");
        }

        Assert.True(result.IsSuccess);
        var table = result.Definition;
        Assert.NotNull(table);
        Assert.Equal("products", table.Name);
        Assert.Equal(7, table.Columns.Count);

        // Формат 1: comment: ...; to_type: ...; to_name: ... (без завершающей ;)
        var id = table.Columns.FirstOrDefault(c => c.Name == "id");
        Assert.NotNull(id);
        Assert.Equal("Уникальный идентификатор товара", id.SqlComment);
        Assert.Equal("product_id", id.ToName);

        // Формат 2: comment(...); to_type(...); to_name(...) (без завершающей ;)
        var legacyName = table.Columns.FirstOrDefault(c => c.Name == "legacy_name");
        Assert.NotNull(legacyName);
        Assert.Equal("Устаревшее имя товара", legacyName.SqlComment);
        Assert.Equal("product_name", legacyName.ToName);

        // Частичный формат: только comment и to_name (без завершающей ;)
        var categoryId = table.Columns.FirstOrDefault(c => c.Name == "category_id");
        Assert.NotNull(categoryId);
        Assert.Equal("ID категории", categoryId.SqlComment);
        Assert.Equal("cat_id", categoryId.ToName);

        // Только comment в скобках (без завершающей ;)
        var description = table.Columns.FirstOrDefault(c => c.Name == "description");
        Assert.NotNull(description);
        Assert.Equal("Полное описание товара", description.SqlComment);
        Assert.Null(description.ToName);

        // type перед comment (без завершающей ;)
        var price = table.Columns.FirstOrDefault(c => c.Name == "price");
        Assert.NotNull(price);
        Assert.Equal("Цена товара", price.SqlComment);
        Assert.Null(price.ToName);

        // rename перед comment (без завершающей ;)
        var stockCount = table.Columns.FirstOrDefault(c => c.Name == "stock_count");
        Assert.NotNull(stockCount);
        Assert.Equal("Количество на складе", stockCount.SqlComment);
        Assert.Equal("quantity", stockCount.ToName);

        // Простой комментарий без служебных слов
        var simpleColumn = table.Columns.FirstOrDefault(c => c.Name == "simple_comment_column");
        Assert.NotNull(simpleColumn);
        Assert.Equal("Простой комментарий без служебных слов", simpleColumn.SqlComment);
        Assert.Null(simpleColumn.ToName);
    }

    [Fact]
    public void Extract_EdgeCasesAndValidation_HandlesCorrectly()
    {
        // Покрывает: null validation, non-table blocks, empty blocks, case sensitivity, mixed case
        Assert.Throws<ArgumentNullException>(() => _extractor.Extract(null!));
        Assert.Throws<ArgumentNullException>(() => _extractor.CanExtract(null!));

        // Non-table block
        var enumBlocks = CreateBlocks("CREATE TYPE status AS ENUM ('active', 'inactive');");
        Assert.False(_extractor.CanExtract(enumBlocks));
        Assert.False(_extractor.Extract(enumBlocks).IsSuccess);

        // Empty block
        Assert.False(_extractor.Extract(CreateBlocks("")).IsSuccess);

        // Uppercase
        var upperBlocks = CreateBlocks(@"
            CREATE TABLE PRODUCTS (
                ID SERIAL PRIMARY KEY, -- Идентификатор
                NAME VARCHAR(100) NOT NULL -- Название
            );
        ");
        var upperResult = _extractor.Extract(upperBlocks);
        Assert.True(upperResult.IsSuccess);
        Assert.Equal("PRODUCTS", upperResult.Definition!.Name);
        Assert.Equal(2, upperResult.Definition.Columns.Count);

        // Mixed case
        var mixedBlocks = CreateBlocks("CrEaTe TaBLe Orders (Id BiGiNt PrImArY KeY);");
        var mixedResult = _extractor.Extract(mixedBlocks);
        Assert.True(mixedResult.IsSuccess);
        Assert.Equal("Orders", mixedResult.Definition!.Name);
    }

    [Fact]
    public void Extract_IdentityGeneratedCollateColumns_ExtractsAdvancedFeatures()
    {
        // Покрывает: GENERATED ALWAYS AS IDENTITY, GENERATED BY DEFAULT AS IDENTITY, 
        // GENERATED ALWAYS AS ... STORED, COLLATE
        var blocks = CreateBlocks(@"
            CREATE TABLE advanced_features (
                id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                sequence_id INTEGER GENERATED BY DEFAULT AS IDENTITY,
                full_name VARCHAR(200) COLLATE ""en_US"" NOT NULL,
                display_name VARCHAR(100) COLLATE en_US,
                total_price NUMERIC(10,2) GENERATED ALWAYS AS (unit_price * quantity) STORED,
                description TEXT,
                status VARCHAR(20) DEFAULT 'active'
            );
        ");

        var result = _extractor.Extract(blocks);
        if (!result.IsSuccess)
        {
            var issues = string.Join(", ", result.ValidationIssues.Select(i => i.Message));
            Assert.Fail($"Extraction failed with issues: {issues}");
        }

        Assert.True(result.IsSuccess);
        var table = result.Definition;
        Assert.NotNull(table);
        Assert.Equal("advanced_features", table.Name);
        Assert.Equal(7, table.Columns.Count);

        // Проверка GENERATED ALWAYS AS IDENTITY
        var id = table.Columns.FirstOrDefault(c => c.Name == "id");
        Assert.NotNull(id);
        Assert.True(id.IsIdentity);
        Assert.Equal("ALWAYS", id.IdentityGeneration);

        // Проверка GENERATED BY DEFAULT AS IDENTITY
        var sequenceId = table.Columns.FirstOrDefault(c => c.Name == "sequence_id");
        Assert.NotNull(sequenceId);
        Assert.True(sequenceId.IsIdentity);
        Assert.Equal("BY DEFAULT", sequenceId.IdentityGeneration);

        // Проверка COLLATE с кавычками
        var fullName = table.Columns.FirstOrDefault(c => c.Name == "full_name");
        Assert.NotNull(fullName);
        Assert.Equal("en_US", fullName.Collation);

        // Проверка COLLATE без кавычек
        var displayName = table.Columns.FirstOrDefault(c => c.Name == "display_name");
        Assert.NotNull(displayName);
        Assert.Equal("en_US", displayName.Collation);

        // Проверка GENERATED ALWAYS AS ... STORED
        var totalPrice = table.Columns.FirstOrDefault(c => c.Name == "total_price");
        Assert.NotNull(totalPrice);
        Assert.True(totalPrice.IsGenerated);
        Assert.Equal("unit_price * quantity", totalPrice.GenerationExpression);

        // Проверка обычной колонки без спецфич
        var description = table.Columns.FirstOrDefault(c => c.Name == "description");
        Assert.NotNull(description);
        Assert.False(description.IsIdentity);
        Assert.Null(description.IdentityGeneration);
        Assert.False(description.IsGenerated);
        Assert.Null(description.GenerationExpression);
        Assert.Null(description.Collation);

        // Проверка колонки с DEFAULT (не должно путаться с IDENTITY)
        var status = table.Columns.FirstOrDefault(c => c.Name == "status");
        Assert.NotNull(status);
        Assert.False(status.IsIdentity);
        Assert.Null(status.IdentityGeneration);
        Assert.Equal("'active'", status.DefaultValue);
    }

    private static IReadOnlyList<SqlBlock> CreateBlocks(string sql, string? comment = null)
    {
        return [new SqlBlock
        {
            Content = sql,
            RawContent = sql,
            HeaderComment = comment,
            StartLine = 1,
            EndLine = sql.Split('\n').Length
        }];
    }
}
