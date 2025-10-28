#!/usr/bin/env dotnet-script
#r "src/PgCs.Core/bin/Debug/net9.0/PgCs.Core.dll"
#r "src/PgCs.SchemaAnalyzer.Tante/bin/Debug/net9.0/PgCs.SchemaAnalyzer.Tante.dll"

using PgCs.SchemaAnalyzer.Tante;
using PgCs.Core.Schema.Analyzer;

var schemaFilePath = "src/PgCs.Core/Example/Schema.sql";

Console.WriteLine("=== Testing ENUM Extraction ===");
Console.WriteLine($"Analyzing file: {schemaFilePath}\n");

var analyzer = new SchemaAnalyzer();
var metadata = await analyzer.AnalyzeFileAsync(schemaFilePath);

Console.WriteLine($"Found {metadata.Enums.Count} ENUM types:\n");

foreach (var enumType in metadata.Enums)
{
    Console.WriteLine($"📦 ENUM: {enumType.Name}");
    if (enumType.Schema is not null)
    {
        Console.WriteLine($"   Schema: {enumType.Schema}");
    }
    Console.WriteLine($"   Values: {string.Join(", ", enumType.Values)}");
    if (enumType.Comment is not null)
    {
        Console.WriteLine($"   Comment: {enumType.Comment}");
    }
    Console.WriteLine();
}

Console.WriteLine($"\n✅ Analysis completed at {metadata.AnalyzedAt:yyyy-MM-dd HH:mm:ss} UTC");
Console.WriteLine($"📁 Source: {string.Join(", ", metadata.SourcePaths)}");

if (metadata.ValidationIssues.Count > 0)
{
    Console.WriteLine($"\n⚠️  Found {metadata.ValidationIssues.Count} validation issues:");
    foreach (var issue in metadata.ValidationIssues)
    {
        Console.WriteLine($"   [{issue.Severity}] {issue.Code}: {issue.Message}");
    }
}
