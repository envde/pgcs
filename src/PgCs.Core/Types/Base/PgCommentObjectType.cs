namespace PgCs.Core.Types.Base;

/// <summary>
/// Тип объекта PostgreSQL для команды COMMENT ON
/// </summary>
/// <remarks>
/// Определяет все типы объектов базы данных, к которым можно применить команду COMMENT ON.
/// 
/// Синтаксис PostgreSQL:
/// <code>
/// COMMENT ON TABLE table_name IS 'comment text';
/// COMMENT ON COLUMN table_name.column_name IS 'comment text';
/// COMMENT ON FUNCTION function_name IS 'comment text';
/// </code>
/// 
/// См. документацию PostgreSQL: https://www.postgresql.org/docs/current/sql-comment.html
/// </remarks>
public enum PgCommentObjectType
{
    /// <summary>
    /// База данных (COMMENT ON DATABASE)
    /// </summary>
    Database,

    /// <summary>
    /// Схема (COMMENT ON SCHEMA)
    /// </summary>
    Schema,

    /// <summary>
    /// Табличное пространство (COMMENT ON TABLESPACE)
    /// </summary>
    Tablespace,

    /// <summary>
    /// Таблица (COMMENT ON TABLE)
    /// </summary>
    Table,

    /// <summary>
    /// Колонка таблицы или представления (COMMENT ON COLUMN)
    /// </summary>
    Column,

    /// <summary>
    /// Представление (COMMENT ON VIEW)
    /// </summary>
    View,

    /// <summary>
    /// Материализованное представление (COMMENT ON MATERIALIZED VIEW)
    /// </summary>
    MaterializedView,

    /// <summary>
    /// Индекс (COMMENT ON INDEX)
    /// </summary>
    Index,

    /// <summary>
    /// Последовательность (COMMENT ON SEQUENCE)
    /// </summary>
    Sequence,

    /// <summary>
    /// Функция (COMMENT ON FUNCTION)
    /// </summary>
    Function,

    /// <summary>
    /// Процедура (COMMENT ON PROCEDURE)
    /// </summary>
    Procedure,

    /// <summary>
    /// Агрегатная функция (COMMENT ON AGGREGATE)
    /// </summary>
    Aggregate,

    /// <summary>
    /// Триггер (COMMENT ON TRIGGER)
    /// </summary>
    Trigger,

    /// <summary>
    /// Ограничение (COMMENT ON CONSTRAINT)
    /// </summary>
    Constraint,

    /// <summary>
    /// Тип данных (COMMENT ON TYPE)
    /// </summary>
    Type,

    /// <summary>
    /// Домен (COMMENT ON DOMAIN)
    /// </summary>
    Domain,

    /// <summary>
    /// Оператор (COMMENT ON OPERATOR)
    /// </summary>
    Operator,

    /// <summary>
    /// Класс операторов (COMMENT ON OPERATOR CLASS)
    /// </summary>
    OperatorClass,

    /// <summary>
    /// Семейство операторов (COMMENT ON OPERATOR FAMILY)
    /// </summary>
    OperatorFamily,

    /// <summary>
    /// Приведение типов (COMMENT ON CAST)
    /// </summary>
    Cast,

    /// <summary>
    /// Расширение (COMMENT ON EXTENSION)
    /// </summary>
    Extension,

    /// <summary>
    /// Обёртка внешних данных (COMMENT ON FOREIGN DATA WRAPPER)
    /// </summary>
    ForeignDataWrapper,

    /// <summary>
    /// Внешний сервер (COMMENT ON SERVER)
    /// </summary>
    ForeignServer,

    /// <summary>
    /// Внешняя таблица (COMMENT ON FOREIGN TABLE)
    /// </summary>
    ForeignTable,

    /// <summary>
    /// Правило (COMMENT ON RULE)
    /// </summary>
    Rule,

    /// <summary>
    /// Роль/пользователь (COMMENT ON ROLE)
    /// </summary>
    Role,

    /// <summary>
    /// Политика безопасности (COMMENT ON POLICY)
    /// </summary>
    Policy,

    /// <summary>
    /// Язык программирования (COMMENT ON LANGUAGE)
    /// </summary>
    Language,

    /// <summary>
    /// Большой объект (COMMENT ON LARGE OBJECT)
    /// </summary>
    LargeObject
}
