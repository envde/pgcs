using System.Text;
using PgCs.Common.CodeGeneration;
using PgCs.Common.QueryAnalyzer.Models.Metadata;
using PgCs.Common.QueryGenerator.Models.Options;
using PgCs.Common.QueryGenerator.Models.Results;
using PgCs.QueryGenerator.Services;

namespace PgCs.QueryGenerator.Generators;

/// <summary>
/// Генератор интерфейса и реализации репозитория запросов
/// </summary>
public sealed class RepositoryGenerator : IRepositoryGenerator
{
    private readonly QuerySyntaxBuilder _syntaxBuilder;

    public RepositoryGenerator(QuerySyntaxBuilder syntaxBuilder)
    {
        _syntaxBuilder = syntaxBuilder;
    }

    public ValueTask<GeneratedInterfaceResult> GenerateInterfaceAsync(
        IReadOnlyList<QueryMetadata> queries,
        QueryGenerationOptions options)
    {
        // TODO: Полная реализация
        var sourceCode = $@"namespace {options.RootNamespace};

public interface {options.RepositoryInterfaceName}
{{
    // TODO: Generated methods
}}";

        var code = new GeneratedCode
        {
            // FilePath removed: //$"{options.RepositoryInterfaceName}.cs",
            SourceCode = sourceCode,
            TypeName = options.RepositoryInterfaceName,
            Namespace = options.RootNamespace,
            CodeType = GeneratedFileType.RepositoryInterface,
        };

        return ValueTask.FromResult(new GeneratedInterfaceResult
        {
            IsSuccess = true,
            InterfaceName = options.RepositoryInterfaceName,
            Code = code,
            MethodCount = queries.Count
        });
    }

    public ValueTask<GeneratedClassResult> GenerateImplementationAsync(
        IReadOnlyList<QueryMetadata> queries,
        QueryGenerationOptions options)
    {
        // TODO: Полная реализация
        var sourceCode = $@"namespace {options.RootNamespace};

public sealed class {options.RepositoryClassName} : {options.RepositoryInterfaceName}
{{
    private readonly NpgsqlDataSource _dataSource;

    public {options.RepositoryClassName}(NpgsqlDataSource dataSource)
    {{
        _dataSource = dataSource;
    }}

    // TODO: Generated methods
}}";

        var code = new GeneratedCode
        {
            // FilePath removed: //$"{options.RepositoryClassName}.cs",
            SourceCode = sourceCode,
            TypeName = options.RepositoryClassName,
            Namespace = options.RootNamespace,
            CodeType = GeneratedFileType.RepositoryImplementation,
        };

        return ValueTask.FromResult(new GeneratedClassResult
        {
            IsSuccess = true,
            ClassName = options.RepositoryClassName,
            Code = code,
            MethodCount = queries.Count
        });
    }
}
