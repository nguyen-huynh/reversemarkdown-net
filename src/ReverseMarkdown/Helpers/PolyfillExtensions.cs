using System;
using System.Collections.Generic;
using System.Linq;


namespace ReverseMarkdown.Helpers;

#if !NET6_0_OR_GREATER
/// <summary>
/// Polyfill extensions for .NET Framework 4.8
/// </summary>
internal static class PolyfillExtensions
{
    /// <summary>
    /// Polyfill for Dictionary.GetValueOrDefault (added in .NET Core 2.0)
    /// </summary>
    public static TValue? GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) where TKey : notnull
    {
        if (dictionary.TryGetValue(key, out var value))
        {
            return value;
        }
        return default;
    }

    /// <summary>
    /// Polyfill for string.ReplaceLineEndings (added in .NET 6)
    /// Important: Always defaults to "\n" (LF) regardless of platform, matching .NET 6+ behavior
    /// </summary>
    public static string ReplaceLineEndings(this string value, string? replacementText = null)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        // .NET 6+ defaults to "\n" when replacement is null, not Environment.NewLine
        // This ensures consistent behavior across frameworks
        replacementText ??= "\n";

        // Handle edge case: if replacementText contains the characters we're replacing,
        // we need to be careful not to double-replace
        if (replacementText.Contains("\r") || replacementText.Contains("\n"))
        {
            // Use a placeholder to avoid double-replacement
            return value
                .Replace("\r\n", "\u0000LINEENDING\u0000")
                .Replace("\r", "\u0000LINEENDING\u0000")
                .Replace("\n", "\u0000LINEENDING\u0000")
                .Replace("\u0000LINEENDING\u0000", replacementText);
        }

        // Simple case: replacement doesn't contain line ending characters
        return value
            .Replace("\r\n", replacementText)
            .Replace("\r", replacementText)
            .Replace("\n", replacementText);
    }

    /// <summary>
    /// Polyfill for Enumerable.DistinctBy (added in .NET 6)
    /// </summary>
    public static IEnumerable<TSource> DistinctBy<TSource, TKey>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector)
    {
        var seenKeys = new HashSet<TKey>();
        foreach (var element in source)
        {
            if (seenKeys.Add(keySelector(element)))
            {
                yield return element;
            }
        }
    }
}
#endif
