namespace PgCs.Core.Types.Base;

/// <summary>
/// Типы операторов в SQL выражениях PostgreSQL
/// </summary>
public enum PgOperatorType
{
    // ==================== Сравнение ====================

    /// <summary>Равно: =</summary>
    Equal,

    /// <summary>Не равно: &lt;&gt; или !=</summary>
    NotEqual,

    /// <summary>Больше: &gt;</summary>
    GreaterThan,

    /// <summary>Меньше: &lt;</summary>
    LessThan,

    /// <summary>Больше или равно: &gt;=</summary>
    GreaterThanOrEqual,

    /// <summary>Меньше или равно: &lt;=</summary>
    LessThanOrEqual,

    // ==================== Логические ====================

    /// <summary>Логическое И: AND</summary>
    And,

    /// <summary>Логическое ИЛИ: OR</summary>
    Or,

    /// <summary>Логическое НЕ: NOT</summary>
    Not,

    // ==================== Арифметические ====================

    /// <summary>Сложение: +</summary>
    Add,

    /// <summary>Вычитание: -</summary>
    Subtract,

    /// <summary>Умножение: *</summary>
    Multiply,

    /// <summary>Деление: /</summary>
    Divide,

    /// <summary>Остаток от деления: %</summary>
    Modulo,

    /// <summary>Возведение в степень: ^</summary>
    Power,

    // ==================== Строковые ====================

    /// <summary>Конкатенация строк: ||</summary>
    Concat,

    /// <summary>Сопоставление с шаблоном: LIKE</summary>
    Like,

    /// <summary>Сопоставление с шаблоном без учёта регистра: ILIKE</summary>
    ILike,

    /// <summary>НЕ сопоставляется с шаблоном: NOT LIKE</summary>
    NotLike,

    /// <summary>НЕ сопоставляется с шаблоном без учёта регистра: NOT ILIKE</summary>
    NotILike,

    /// <summary>Сопоставление с SQL регулярным выражением: SIMILAR TO</summary>
    SimilarTo,

    /// <summary>НЕ сопоставляется с SQL регулярным выражением: NOT SIMILAR TO</summary>
    NotSimilarTo,

    // ==================== NULL проверки ====================

    /// <summary>Проверка на NULL: IS NULL</summary>
    IsNull,

    /// <summary>Проверка на NOT NULL: IS NOT NULL</summary>
    IsNotNull,

    /// <summary>Отличается (включая NULL): IS DISTINCT FROM</summary>
    IsDistinctFrom,

    /// <summary>Не отличается (включая NULL): IS NOT DISTINCT FROM</summary>
    IsNotDistinctFrom,

    // ==================== IN/EXISTS ====================

    /// <summary>Проверка вхождения в список: IN</summary>
    In,

    /// <summary>Проверка НЕ вхождения в список: NOT IN</summary>
    NotIn,

    /// <summary>Проверка существования подзапроса: EXISTS</summary>
    Exists,

    /// <summary>Проверка НЕ существования подзапроса: NOT EXISTS</summary>
    NotExists,

    /// <summary>Сравнение с ANY: = ANY, &gt; ANY, etc.</summary>
    Any,

    /// <summary>Сравнение с ALL: = ALL, &gt; ALL, etc.</summary>
    All,

    /// <summary>Сравнение с SOME (синоним ANY): = SOME</summary>
    Some,

    // ==================== BETWEEN ====================

    /// <summary>Проверка диапазона: BETWEEN</summary>
    Between,

    /// <summary>Проверка НЕ в диапазоне: NOT BETWEEN</summary>
    NotBetween,

    // ==================== PostgreSQL специфичные ====================

    /// <summary>Приведение типа: ::</summary>
    Cast,

    /// <summary>JSON извлечение поля: -&gt;</summary>
    JsonExtract,

    /// <summary>JSON извлечение поля как текст: -&gt;&gt;</summary>
    JsonExtractText,

    /// <summary>JSON извлечение по пути: #&gt;</summary>
    JsonPathExtract,

    /// <summary>JSON извлечение по пути как текст: #&gt;&gt;</summary>
    JsonPathExtractText,

    /// <summary>Содержит (JSON, массивы): @&gt;</summary>
    Contains,

    /// <summary>Содержится в (JSON, массивы): &lt;@</summary>
    ContainedBy,

    /// <summary>Пересечение (массивы, диапазоны): &amp;&amp;</summary>
    Overlap,

    /// <summary>Проверка существования ключа в JSON: ?</summary>
    JsonKeyExists,

    /// <summary>Проверка существования любого ключа: ?|</summary>
    JsonKeyExistsAny,

    /// <summary>Проверка существования всех ключей: ?&amp;</summary>
    JsonKeyExistsAll,

    // ==================== Регулярные выражения ====================

    /// <summary>Совпадение с регулярным выражением: ~</summary>
    RegexMatch,

    /// <summary>Совпадение с регулярным выражением без учёта регистра: ~*</summary>
    RegexMatchCaseInsensitive,

    /// <summary>НЕ совпадает с регулярным выражением: !~</summary>
    RegexNotMatch,

    /// <summary>НЕ совпадает с регулярным выражением без учёта регистра: !~*</summary>
    RegexNotMatchCaseInsensitive,

    // ==================== Full-Text Search ====================

    /// <summary>Полнотекстовый поиск: @@</summary>
    TextSearchMatch,

    // ==================== Range операторы ====================

    /// <summary>Строго слева от: &lt;&lt;</summary>
    StrictlyLeftOf,

    /// <summary>Строго справа от: &gt;&gt;</summary>
    StrictlyRightOf,

    /// <summary>Не расширяется вправо от: &amp;&lt;</summary>
    DoesNotExtendRightOf,

    /// <summary>Не расширяется влево от: &amp;&gt;</summary>
    DoesNotExtendLeftOf,

    /// <summary>Смежные диапазоны: -|-</summary>
    Adjacent,

    // ==================== Битовые операторы ====================

    /// <summary>Побитовое И: &amp;</summary>
    BitwiseAnd,

    /// <summary>Побитовое ИЛИ: |</summary>
    BitwiseOr,

    /// <summary>Побитовое исключающее ИЛИ: #</summary>
    BitwiseXor,

    /// <summary>Побитовое НЕ: ~</summary>
    BitwiseNot,

    /// <summary>Битовый сдвиг влево: &lt;&lt;</summary>
    BitwiseShiftLeft,

    /// <summary>Битовый сдвиг вправо: &gt;&gt;</summary>
    BitwiseShiftRight,

    // ==================== Геометрические операторы ====================

    /// <summary>Расстояние: &lt;-&gt;</summary>
    Distance,

    // ==================== Специальные ====================

    /// <summary>IS TRUE проверка</summary>
    IsTrue,

    /// <summary>IS FALSE проверка</summary>
    IsFalse,

    /// <summary>IS UNKNOWN проверка</summary>
    IsUnknown,

    /// <summary>IS NOT TRUE проверка</summary>
    IsNotTrue,

    /// <summary>IS NOT FALSE проверка</summary>
    IsNotFalse,

    /// <summary>IS NOT UNKNOWN проверка</summary>
    IsNotUnknown
}
