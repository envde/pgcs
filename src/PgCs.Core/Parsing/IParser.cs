using PgCs.Core.Types;

namespace PgCs.Core.Parsing;

/// <summary>
/// Обобщённый интерфейс парсера для преобразования SQL токенов в строго типизированные объекты PostgreSQL
/// </summary>
/// <typeparam name="T">Тип объекта PostgreSQL для парсинга (PgTable, PgFunction и т.д.)</typeparam>
public interface IParser<T> where T : PgObject
{
    /// <summary>
    /// Выполняет парсинг SQL токенов в объект PostgreSQL
    /// </summary>
    /// <param name="context">Контекст парсера с возможностями навигации по токенам</param>
    /// <returns>Результат парсинга, содержащий объект или информацию об ошибке</returns>
    ParseResult<T> Parse(ParserContext context);
}
