namespace PgCs.Common.Utils;

/// <summary>
/// Утилиты для работы с коллекциями.
/// Содержит общие методы для дедупликации, группировки и других операций.
/// </summary>
public static class CollectionHelpers
{
    /// <summary>
    /// Удаляет дубликаты из коллекции на основе ключа группировки.
    /// Для каждой группы возвращается первый элемент.
    /// </summary>
    /// <typeparam name="TSource">Тип элементов в коллекции</typeparam>
    /// <typeparam name="TKey">Тип ключа для группировки</typeparam>
    /// <param name="source">Исходная коллекция</param>
    /// <param name="keySelector">Функция для выбора ключа группировки</param>
    /// <returns>Массив уникальных элементов</returns>
    public static IReadOnlyList<TSource> DeduplicateBy<TSource, TKey>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector)
    {
        return source
            .GroupBy(keySelector)
            .Select(g => g.First())
            .ToArray();
    }
}
