namespace PgCs.Core.Types.Base;

/// <summary>
/// Вид пользовательского типа данных в PostgreSQL
/// </summary>
/// <remarks>
/// PostgreSQL поддерживает создание пользовательских типов данных помимо встроенных.
/// </remarks>
public enum PgTypeKind
{
    /// <summary>
    /// ENUM - перечисление с фиксированным набором значений
    /// </summary>
    /// <example>
    /// CREATE TYPE user_status AS ENUM ('active', 'inactive', 'banned');
    /// </example>
    /// <remarks>
    /// - Упорядоченный набор строковых значений
    /// - Безопасность типов на уровне БД
    /// - Эффективное хранение (4 байта вместо строки)
    /// </remarks>
    Enum,

    /// <summary>
    /// Composite - составной тип с несколькими именованными полями
    /// </summary>
    /// <example>
    /// CREATE TYPE address AS (street varchar, city varchar, zip_code varchar);
    /// </example>
    /// <remarks>
    /// - Подобен struct/record в других языках
    /// - Может быть использован как тип колонки
    /// - Поддерживает вложенные составные типы
    /// </remarks>
    Composite,

    /// <summary>
    /// Domain - ограниченный базовый тип с дополнительными проверками
    /// </summary>
    /// <example>
    /// CREATE DOMAIN email AS varchar CHECK (VALUE ~ '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z]{2,}$');
    /// </example>
    /// <remarks>
    /// - Базируется на существующем типе
    /// - Добавляет ограничения (CHECK constraints)
    /// - Может иметь значение по умолчанию
    /// - Может быть NOT NULL
    /// </remarks>
    Domain,

    /// <summary>
    /// Range - тип диапазона значений
    /// </summary>
    /// <example>
    /// daterange, int4range, tstzrange
    /// CREATE TYPE custom_range AS RANGE (subtype = integer);
    /// </example>
    /// <remarks>
    /// - Представляет интервал между двумя значениями
    /// - Поддерживает операции над диапазонами (пересечение, объединение)
    /// - Может быть ограниченным, полуограниченным или неограниченным
    /// </remarks>
    Range
}
