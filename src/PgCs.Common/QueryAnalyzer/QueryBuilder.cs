using System.Text;

namespace PgCs.Common.QueryAnalyzer;

/// <summary>
/// Fluent API для построения SQL запросов
/// </summary>
public class QueryBuilder
{
    private string? _queryName;
    private QueryBuilderType _queryType;
    private readonly List<string> _selectColumns = new();
    private string? _fromTable;
    private readonly List<string> _joinClauses = new();
    private readonly List<string> _whereClauses = new();
    private readonly List<string> _orderByClauses = new();
    private readonly List<string> _groupByClauses = new();
    private string? _havingClause;
    private int? _limit;
    private int? _offset;
    private readonly List<(string Column, string Parameter)> _insertColumns = new();
    private readonly List<(string Column, string Value)> _updateColumns = new();
    private readonly List<string> _returningColumns = new();
    private bool _distinct;
    private readonly List<string> _ctes = new();
    private string? _returnCardinality;
    
    private QueryBuilder() { }

    /// <summary>
    /// Создать новый SELECT запрос
    /// </summary>
    public static QueryBuilder Select(params string[] columns)
    {
        var builder = new QueryBuilder { _queryType = QueryBuilderType.Select };
        if (columns.Length > 0)
        {
            builder._selectColumns.AddRange(columns);
        }
        return builder;
    }

    /// <summary>
    /// Создать новый INSERT запрос
    /// </summary>
    public static QueryBuilder Insert(string tableName)
    {
        return new QueryBuilder 
        { 
            _queryType = QueryBuilderType.Insert,
            _fromTable = tableName
        };
    }

    /// <summary>
    /// Создать новый UPDATE запрос
    /// </summary>
    public static QueryBuilder Update(string tableName)
    {
        return new QueryBuilder 
        { 
            _queryType = QueryBuilderType.Update,
            _fromTable = tableName
        };
    }

    /// <summary>
    /// Создать новый DELETE запрос
    /// </summary>
    public static QueryBuilder Delete(string tableName)
    {
        return new QueryBuilder 
        { 
            _queryType = QueryBuilderType.Delete,
            _fromTable = tableName
        };
    }

    /// <summary>
    /// Указать имя запроса для генерации метода (-- name: GetUser)
    /// </summary>
    public QueryBuilder Named(string queryName)
    {
        _queryName = queryName;
        return this;
    }

    /// <summary>
    /// Указать тип возврата (:one, :many, :exec, :execrows)
    /// </summary>
    public QueryBuilder Returns(string cardinality)
    {
        _returnCardinality = cardinality;
        return this;
    }

    /// <summary>
    /// Возвращает один объект (:one)
    /// </summary>
    public QueryBuilder One()
    {
        _returnCardinality = "one";
        return this;
    }

    /// <summary>
    /// Возвращает множество объектов (:many)
    /// </summary>
    public QueryBuilder Many()
    {
        _returnCardinality = "many";
        return this;
    }

    /// <summary>
    /// Выполняет без возврата (:exec)
    /// </summary>
    public QueryBuilder Exec()
    {
        _returnCardinality = "exec";
        return this;
    }

    /// <summary>
    /// Выполняет с возвратом количества строк (:execrows)
    /// </summary>
    public QueryBuilder ExecRows()
    {
        _returnCardinality = "execrows";
        return this;
    }

    /// <summary>
    /// Добавить колонки для SELECT
    /// </summary>
    public QueryBuilder Columns(params string[] columns)
    {
        _selectColumns.AddRange(columns);
        return this;
    }

    /// <summary>
    /// SELECT DISTINCT
    /// </summary>
    public QueryBuilder Distinct()
    {
        _distinct = true;
        return this;
    }

    /// <summary>
    /// Добавить все колонки (SELECT *)
    /// </summary>
    public QueryBuilder AllColumns()
    {
        _selectColumns.Clear();
        _selectColumns.Add("*");
        return this;
    }

    /// <summary>
    /// Указать таблицу (FROM table)
    /// </summary>
    public QueryBuilder From(string tableName)
    {
        _fromTable = tableName;
        return this;
    }

    /// <summary>
    /// INNER JOIN
    /// </summary>
    public QueryBuilder InnerJoin(string table, string onCondition)
    {
        _joinClauses.Add($"INNER JOIN {table} ON {onCondition}");
        return this;
    }

    /// <summary>
    /// LEFT JOIN
    /// </summary>
    public QueryBuilder LeftJoin(string table, string onCondition)
    {
        _joinClauses.Add($"LEFT JOIN {table} ON {onCondition}");
        return this;
    }

    /// <summary>
    /// RIGHT JOIN
    /// </summary>
    public QueryBuilder RightJoin(string table, string onCondition)
    {
        _joinClauses.Add($"RIGHT JOIN {table} ON {onCondition}");
        return this;
    }

    /// <summary>
    /// FULL OUTER JOIN
    /// </summary>
    public QueryBuilder FullJoin(string table, string onCondition)
    {
        _joinClauses.Add($"FULL OUTER JOIN {table} ON {onCondition}");
        return this;
    }

    /// <summary>
    /// Добавить WHERE условие
    /// </summary>
    public QueryBuilder Where(string condition)
    {
        _whereClauses.Add(condition);
        return this;
    }

    /// <summary>
    /// WHERE с параметром (WHERE id = $1)
    /// </summary>
    public QueryBuilder WhereEquals(string column, int parameterIndex)
    {
        _whereClauses.Add($"{column} = ${parameterIndex}");
        return this;
    }

    /// <summary>
    /// WHERE IN (WHERE status = ANY($1))
    /// </summary>
    public QueryBuilder WhereIn(string column, int parameterIndex, string? arrayType = null)
    {
        var condition = arrayType != null 
            ? $"{column} = ANY(${parameterIndex}::{arrayType}[])"
            : $"{column} = ANY(${parameterIndex})";
        _whereClauses.Add(condition);
        return this;
    }

    /// <summary>
    /// WHERE BETWEEN
    /// </summary>
    public QueryBuilder WhereBetween(string column, int startParam, int endParam)
    {
        _whereClauses.Add($"{column} BETWEEN ${startParam} AND ${endParam}");
        return this;
    }

    /// <summary>
    /// WHERE IS NULL
    /// </summary>
    public QueryBuilder WhereNull(string column)
    {
        _whereClauses.Add($"{column} IS NULL");
        return this;
    }

    /// <summary>
    /// WHERE IS NOT NULL
    /// </summary>
    public QueryBuilder WhereNotNull(string column)
    {
        _whereClauses.Add($"{column} IS NOT NULL");
        return this;
    }

    /// <summary>
    /// WHERE LIKE
    /// </summary>
    public QueryBuilder WhereLike(string column, int parameterIndex)
    {
        _whereClauses.Add($"{column} LIKE ${parameterIndex}");
        return this;
    }

    /// <summary>
    /// WHERE ILIKE (case-insensitive)
    /// </summary>
    public QueryBuilder WhereILike(string column, int parameterIndex)
    {
        _whereClauses.Add($"{column} ILIKE ${parameterIndex}");
        return this;
    }

    /// <summary>
    /// WHERE для JSONB (preferences @> $1::jsonb)
    /// </summary>
    public QueryBuilder WhereJsonbContains(string column, int parameterIndex)
    {
        _whereClauses.Add($"{column} @> ${parameterIndex}::jsonb");
        return this;
    }

    /// <summary>
    /// WHERE для массивов (tags && $1::text[])
    /// </summary>
    public QueryBuilder WhereArrayOverlaps(string column, int parameterIndex, string arrayType)
    {
        _whereClauses.Add($"{column} && ${parameterIndex}::{arrayType}[]");
        return this;
    }

    /// <summary>
    /// Добавить ORDER BY
    /// </summary>
    public QueryBuilder OrderBy(params string[] columns)
    {
        _orderByClauses.AddRange(columns);
        return this;
    }

    /// <summary>
    /// ORDER BY DESC
    /// </summary>
    public QueryBuilder OrderByDesc(params string[] columns)
    {
        _orderByClauses.AddRange(columns.Select(c => $"{c} DESC"));
        return this;
    }

    /// <summary>
    /// GROUP BY
    /// </summary>
    public QueryBuilder GroupBy(params string[] columns)
    {
        _groupByClauses.AddRange(columns);
        return this;
    }

    /// <summary>
    /// HAVING
    /// </summary>
    public QueryBuilder Having(string condition)
    {
        _havingClause = condition;
        return this;
    }

    /// <summary>
    /// LIMIT
    /// </summary>
    public QueryBuilder Limit(int limit)
    {
        _limit = limit;
        return this;
    }

    /// <summary>
    /// LIMIT с параметром (LIMIT $1)
    /// </summary>
    public QueryBuilder LimitParam(int parameterIndex)
    {
        _limit = -parameterIndex; // отрицательное значение означает параметр
        return this;
    }

    /// <summary>
    /// OFFSET
    /// </summary>
    public QueryBuilder Offset(int offset)
    {
        _offset = offset;
        return this;
    }

    /// <summary>
    /// OFFSET с параметром (OFFSET $1)
    /// </summary>
    public QueryBuilder OffsetParam(int parameterIndex)
    {
        _offset = -parameterIndex; // отрицательное значение означает параметр
        return this;
    }

    /// <summary>
    /// Добавить WITH CTE (Common Table Expression)
    /// </summary>
    public QueryBuilder WithCte(string cteName, string cteQuery)
    {
        _ctes.Add($"{cteName} AS ({cteQuery})");
        return this;
    }

    /// <summary>
    /// Добавить колонки для INSERT VALUES
    /// </summary>
    public QueryBuilder Values(params (string Column, string Parameter)[] values)
    {
        _insertColumns.AddRange(values);
        return this;
    }

    /// <summary>
    /// Добавить колонки для INSERT с автонумерацией параметров
    /// </summary>
    public QueryBuilder Values(params string[] columns)
    {
        for (int i = 0; i < columns.Length; i++)
        {
            _insertColumns.Add((columns[i], $"${i + 1}"));
        }
        return this;
    }

    /// <summary>
    /// Добавить SET для UPDATE
    /// </summary>
    public QueryBuilder Set(string column, string value)
    {
        _updateColumns.Add((column, value));
        return this;
    }

    /// <summary>
    /// Добавить SET с параметром (SET name = $1)
    /// </summary>
    public QueryBuilder SetParam(string column, int parameterIndex)
    {
        _updateColumns.Add((column, $"${parameterIndex}"));
        return this;
    }

    /// <summary>
    /// SET для обновления timestamp (SET updated_at = NOW())
    /// </summary>
    public QueryBuilder SetNow(string column = "updated_at")
    {
        _updateColumns.Add((column, "NOW()"));
        return this;
    }

    /// <summary>
    /// RETURNING колонки
    /// </summary>
    public QueryBuilder Returning(params string[] columns)
    {
        _returningColumns.AddRange(columns);
        return this;
    }

    /// <summary>
    /// RETURNING * (все колонки)
    /// </summary>
    public QueryBuilder ReturningAll()
    {
        _returningColumns.Clear();
        _returningColumns.Add("*");
        return this;
    }

    /// <summary>
    /// Построить SQL запрос
    /// </summary>
    public string BuildSql()
    {
        var sql = new StringBuilder();

        // WITH CTEs
        if (_ctes.Count > 0)
        {
            sql.AppendLine($"WITH {string.Join(",\n     ", _ctes)}");
        }

        switch (_queryType)
        {
            case QueryBuilderType.Select:
                BuildSelectQuery(sql);
                break;
            case QueryBuilderType.Insert:
                BuildInsertQuery(sql);
                break;
            case QueryBuilderType.Update:
                BuildUpdateQuery(sql);
                break;
            case QueryBuilderType.Delete:
                BuildDeleteQuery(sql);
                break;
        }

        return sql.ToString().TrimEnd();
    }

    /// <summary>
    /// Построить полный запрос с аннотациями sqlc
    /// </summary>
    public string Build()
    {
        var result = new StringBuilder();

        // Добавить комментарий с именем и кардинальностью
        if (!string.IsNullOrEmpty(_queryName))
        {
            result.Append($"-- name: {_queryName}");
            if (!string.IsNullOrEmpty(_returnCardinality))
            {
                result.Append($" :{_returnCardinality}");
            }
            result.AppendLine();
        }

        // Добавить SQL
        result.Append(BuildSql());

        return result.ToString();
    }

    private void BuildSelectQuery(StringBuilder sql)
    {
        sql.Append("SELECT ");
        
        if (_distinct)
        {
            sql.Append("DISTINCT ");
        }

        // Колонки
        if (_selectColumns.Count == 0)
        {
            sql.Append("*");
        }
        else
        {
            sql.Append(string.Join(",\n       ", _selectColumns));
        }

        // FROM
        if (!string.IsNullOrEmpty(_fromTable))
        {
            sql.AppendLine();
            sql.Append($"FROM {_fromTable}");
        }

        // JOINs
        foreach (var join in _joinClauses)
        {
            sql.AppendLine();
            sql.Append($"    {join}");
        }

        // WHERE
        if (_whereClauses.Count > 0)
        {
            sql.AppendLine();
            sql.Append($"WHERE {string.Join("\n  AND ", _whereClauses)}");
        }

        // GROUP BY
        if (_groupByClauses.Count > 0)
        {
            sql.AppendLine();
            sql.Append($"GROUP BY {string.Join(", ", _groupByClauses)}");
        }

        // HAVING
        if (!string.IsNullOrEmpty(_havingClause))
        {
            sql.AppendLine();
            sql.Append($"HAVING {_havingClause}");
        }

        // ORDER BY
        if (_orderByClauses.Count > 0)
        {
            sql.AppendLine();
            sql.Append($"ORDER BY {string.Join(", ", _orderByClauses)}");
        }

        // LIMIT
        if (_limit.HasValue)
        {
            sql.AppendLine();
            sql.Append(_limit.Value < 0 
                ? $"LIMIT ${Math.Abs(_limit.Value)}" 
                : $"LIMIT {_limit.Value}");
        }

        // OFFSET
        if (_offset.HasValue)
        {
            sql.AppendLine();
            sql.Append(_offset.Value < 0 
                ? $"OFFSET ${Math.Abs(_offset.Value)}" 
                : $"OFFSET {_offset.Value}");
        }
    }

    private void BuildInsertQuery(StringBuilder sql)
    {
        sql.Append($"INSERT INTO {_fromTable}");

        if (_insertColumns.Count > 0)
        {
            sql.AppendLine(" (");
            sql.Append("    ");
            sql.Append(string.Join(",\n    ", _insertColumns.Select(c => c.Column)));
            sql.AppendLine();
            sql.Append(")");
            sql.AppendLine();
            sql.Append("VALUES (");
            sql.Append(string.Join(", ", _insertColumns.Select(c => c.Parameter)));
            sql.Append(")");
        }

        // RETURNING
        if (_returningColumns.Count > 0)
        {
            sql.Append($" RETURNING {string.Join(", ", _returningColumns)}");
        }
    }

    private void BuildUpdateQuery(StringBuilder sql)
    {
        sql.Append($"UPDATE {_fromTable}");
        sql.AppendLine();

        if (_updateColumns.Count > 0)
        {
            sql.Append("SET ");
            sql.Append(string.Join(",\n    ", _updateColumns.Select(c => $"{c.Column} = {c.Value}")));
        }

        // WHERE
        if (_whereClauses.Count > 0)
        {
            sql.AppendLine();
            sql.Append($"WHERE {string.Join("\n  AND ", _whereClauses)}");
        }

        // RETURNING
        if (_returningColumns.Count > 0)
        {
            sql.AppendLine();
            sql.Append($"RETURNING {string.Join(", ", _returningColumns)}");
        }
    }

    private void BuildDeleteQuery(StringBuilder sql)
    {
        sql.Append($"DELETE FROM {_fromTable}");

        // WHERE
        if (_whereClauses.Count > 0)
        {
            sql.AppendLine();
            sql.Append($"WHERE {string.Join("\n  AND ", _whereClauses)}");
        }

        // RETURNING
        if (_returningColumns.Count > 0)
        {
            sql.AppendLine();
            sql.Append($"RETURNING {string.Join(", ", _returningColumns)}");
        }
    }
}

/// <summary>
/// Тип SQL запроса
/// </summary>
internal enum QueryBuilderType
{
    Select,
    Insert,
    Update,
    Delete
}
