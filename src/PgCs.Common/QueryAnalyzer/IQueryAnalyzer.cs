namespace PgCs.Common.QueryAnalyzer;

public interface IQueryAnalyzer
{
    /// <summary>
    /// Анализирует SQL файл и извлекает все запросы с метаданными
    /// </summary>
    /// <param name="sqlFilePath">Путь к SQL файлу</param>
    /// <returns>Список проанализированных запросов</returns>
    ValueTask<IReadOnlyList<QueryMetadata>> AnalyzeFileAsync(string sqlFilePath);

    /// <summary>
    /// Анализирует отдельный SQL запрос
    /// </summary>
    /// <param name="sqlQuery">SQL запрос с комментариями</param>
    /// <returns>Метаданные запроса</returns>
    QueryMetadata AnalyzeQuery(string sqlQuery);

    /// <summary>
    /// Извлекает параметры из SQL запроса
    /// </summary>
    /// <param name="sqlQuery">SQL запрос</param>
    /// <returns>Список параметров</returns>
    IReadOnlyList<QueryParameter> ExtractParameters(string sqlQuery);

    /// <summary>
    /// Определяет тип возвращаемого значения на основе SELECT запроса
    /// </summary>
    /// <param name="sqlQuery">SELECT запрос</param>
    /// <returns>Информация о возвращаемых колонках</returns>
    ReturnTypeInfo InferReturnType(string sqlQuery);

    /// <summary>
    /// Парсит комментарии sqlc формата (-- name: GetUser :one)
    /// </summary>
    /// <param name="comments">Комментарии перед запросом</param>
    /// <returns>Метаданные из комментариев</returns>
    QueryAnnotation ParseAnnotations(string comments);
}
