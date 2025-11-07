namespace PgCs.Core.Types.Base;

/// <summary>
/// Типы агрегатных функций в PostgreSQL
/// </summary>
public enum PgAggregateFunction
{
    /// <summary>Подсчёт количества строк: COUNT(*) или COUNT(column)</summary>
    Count,

    /// <summary>Сумма значений: SUM(column)</summary>
    Sum,

    /// <summary>Среднее значение: AVG(column)</summary>
    Average,

    /// <summary>Минимальное значение: MIN(column)</summary>
    Min,

    /// <summary>Максимальное значение: MAX(column)</summary>
    Max,

    /// <summary>Логическое И для всех значений: BOOL_AND(boolean_column)</summary>
    BoolAnd,

    /// <summary>Логическое ИЛИ для всех значений: BOOL_OR(boolean_column)</summary>
    BoolOr,

    /// <summary>Объединение массивов: ARRAY_AGG(column)</summary>
    ArrayAgg,

    /// <summary>Объединение строк: STRING_AGG(column, delimiter)</summary>
    StringAgg,

    /// <summary>JSON объект из пар ключ-значение: JSON_OBJECT_AGG(key, value)</summary>
    JsonObjectAgg,

    /// <summary>JSON массив: JSON_AGG(value)</summary>
    JsonAgg,

    /// <summary>JSONB объект из пар ключ-значение: JSONB_OBJECT_AGG(key, value)</summary>
    JsonbObjectAgg,

    /// <summary>JSONB массив: JSONB_AGG(value)</summary>
    JsonbAgg,

    /// <summary>Битовое И: BIT_AND(int_column)</summary>
    BitAnd,

    /// <summary>Битовое ИЛИ: BIT_OR(int_column)</summary>
    BitOr,

    /// <summary>Среднеквадратичное отклонение: STDDEV(column)</summary>
    StdDev,

    /// <summary>Дисперсия: VARIANCE(column)</summary>
    Variance,

    /// <summary>Корреляция: CORR(y, x)</summary>
    Correlation,

    /// <summary>Коэффициент линейной регрессии: REGR_SLOPE(y, x)</summary>
    RegressionSlope,

    /// <summary>Интерсепт линейной регрессии: REGR_INTERCEPT(y, x)</summary>
    RegressionIntercept,

    /// <summary>Пользовательская агрегатная функция</summary>
    Custom
}
