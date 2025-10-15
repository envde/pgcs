using System.Text.RegularExpressions;
using PgCs.Common.SchemaAnalyzer;

namespace PgCs.SchemaAnalyzer.Extractors;

/// <summary>
/// Извлекает информацию о партициях
/// </summary>
internal sealed partial class PartitionExtractor
{
    [GeneratedRegex(@"CREATE\s+TABLE\s+([a-zA-Z_][a-zA-Z0-9_]*)\s+PARTITION\s+OF\s+([a-zA-Z_][a-zA-Z0-9_]*)\s+FOR\s+VALUES\s+(FROM\s*\((.*?)\)\s+TO\s*\((.*?)\)|IN\s*\((.*?)\)|WITH\s*\(MODULUS\s+(\d+),\s*REMAINDER\s+(\d+)\))", 
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled)]
    private static partial Regex PartitionPatternRegex();

    public IReadOnlyList<PartitionDefinition> ExtractPartitions(string sqlScript)
    {
        if (string.IsNullOrWhiteSpace(sqlScript))
            return [];

        var partitions = new List<PartitionDefinition>();
        var matches = PartitionPatternRegex().Matches(sqlScript);

        foreach (Match match in matches)
        {
            var partition = ParsePartition(match);
            if (partition is not null)
            {
                partitions.Add(partition);
            }
        }

        return partitions;
    }

    private PartitionDefinition? ParsePartition(Match match)
    {
        var partitionName = match.Groups[1].Value.Trim();

        // RANGE partition
        if (match.Groups[4].Success && match.Groups[5].Success)
        {
            return new PartitionDefinition
            {
                Name = partitionName,
                FromValue = match.Groups[4].Value.Trim(),
                ToValue = match.Groups[5].Value.Trim()
            };
        }

        // LIST partition
        if (match.Groups[6].Success)
        {
            var inValues = match.Groups[6].Value
                .Split(',')
                .Select(v => v.Trim().Trim('\''))
                .ToArray();

            return new PartitionDefinition
            {
                Name = partitionName,
                InValues = inValues
            };
        }

        // HASH partition
        if (match.Groups[7].Success && match.Groups[8].Success)
        {
            return new PartitionDefinition
            {
                Name = partitionName,
                Modulus = int.Parse(match.Groups[7].Value),
                Remainder = int.Parse(match.Groups[8].Value)
            };
        }

        return null;
    }
}