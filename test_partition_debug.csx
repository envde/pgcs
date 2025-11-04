#!/usr/bin/env dotnet-script
#r "nuget: System.Text.RegularExpressions, 4.3.1"

using System.Text.RegularExpressions;

// Test the regex pattern
var pattern = new Regex(@"'([^']*)'", RegexOptions.Compiled);
var text = "('active', 'pending', 'active', 'processing')";

Console.WriteLine($"Input: {text}");
Console.WriteLine("Extracted values:");

var matches = pattern.Matches(text);
var values = new List<string>();

foreach (Match match in matches)
{
    if (match.Groups.Count > 1)
    {
        var value = match.Groups[1].Value;
        values.Add(value);
        Console.WriteLine($"  - '{value}'");
    }
}

Console.WriteLine($"\nTotal values: {values.Count}");

// Check for duplicates
var duplicates = values
    .GroupBy(v => v)
    .Where(g => g.Count() > 1)
    .Select(g => g.Key)
    .ToList();

Console.WriteLine($"Duplicates: {string.Join(", ", duplicates)}");
Console.WriteLine($"Has duplicates: {duplicates.Count > 0}");
