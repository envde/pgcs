#r "src/PgCs.QueryAnalyzer/bin/Debug/net9.0/PgCs.QueryAnalyzer.dll"
#r "src/PgCs.Common/bin/Debug/net9.0/PgCs.Common.dll"

using PgCs.QueryAnalyzer.Parsing;

var sql = @"
-- name: GetUserById :one
-- summary: Retrieves a user by their unique identifier
-- param: id The unique user identifier (UUID)
-- returns: User object if found, null otherwise
SELECT id, username, email, active
FROM users
WHERE id = $1;
";

var parser = new AnnotationParser();
if (parser.HasAnnotation(sql))
{
    var annotation = parser.Parse(sql);
    Console.WriteLine($"Name: {annotation.Name}");
    Console.WriteLine($"Cardinality: {annotation.Cardinality}");
    Console.WriteLine($"Summary: {annotation.Summary}");
    Console.WriteLine($"Returns: {annotation.Returns}");
    Console.WriteLine($"Parameters ({annotation.ParameterDescriptions?.Count ?? 0}):");
    if (annotation.ParameterDescriptions != null)
    {
        foreach (var p in annotation.ParameterDescriptions)
        {
            Console.WriteLine($"  - {p.Key}: {p.Value}");
        }
    }
}
else
{
    Console.WriteLine("No annotation found");
}
