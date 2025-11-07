namespace PgCs.Core.Types.Base;

/// <summary>
/// Типы литеральных значений в SQL выражениях PostgreSQL
/// </summary>
public enum PgLiteralType
{
    /// <summary>NULL значение</summary>
    Null,

    /// <summary>Boolean значение: TRUE, FALSE</summary>
    Boolean,

    /// <summary>Целочисленное значение: 42, -100, 0</summary>
    Integer,

    /// <summary>Числовое значение с плавающей точкой: 3.14, 2.5e10</summary>
    Numeric,

    /// <summary>Строковое значение: 'text', $$text$$</summary>
    String,

    /// <summary>Дата: DATE '2025-11-07'</summary>
    Date,

    /// <summary>Время: TIME '14:30:00'</summary>
    Time,

    /// <summary>Время с часовым поясом: TIME '14:30:00+03'</summary>
    TimeWithTimeZone,

    /// <summary>Дата и время: TIMESTAMP '2025-11-07 14:30:00'</summary>
    Timestamp,

    /// <summary>Дата и время с часовым поясом: TIMESTAMPTZ '2025-11-07 14:30:00+03'</summary>
    TimestampWithTimeZone,

    /// <summary>Интервал времени: INTERVAL '1 day'</summary>
    Interval,

    /// <summary>Массив: ARRAY[1, 2, 3] или '{1,2,3}'</summary>
    Array,

    /// <summary>JSON объект: '{"key": "value"}'::json</summary>
    Json,

    /// <summary>JSONB объект: '{"key": "value"}'::jsonb</summary>
    JsonB,

    /// <summary>UUID значение: 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11'::uuid</summary>
    Uuid,

    /// <summary>Битовая строка: B'10101' или X'1A2B'</summary>
    BitString,

    /// <summary>Бинарные данные: '\xDEADBEEF'::bytea</summary>
    Bytea,

    /// <summary>XML документ: XMLPARSE(DOCUMENT '&lt;root/&gt;')</summary>
    Xml,

    /// <summary>Геометрический тип (point, line, etc.): POINT(1, 2)</summary>
    Geometric,

    /// <summary>Сетевой адрес (inet, cidr, macaddr): '192.168.1.1'::inet</summary>
    Network,

    /// <summary>Диапазон: '[1,10)'::int4range</summary>
    Range,

    /// <summary>Тип money: '$100.50'::money</summary>
    Money
}
