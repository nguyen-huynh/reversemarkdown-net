using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace ReverseMarkdown.Helpers;

public static class StringExtensions {
    public static string Chomp(this string content)
    {
        return content
            .ReplaceLineEndings(string.Empty)
            .Trim();
    }

#if NET5_0_OR_GREATER
    internal static LineSplitEnumerator ReadLines(this string content)
    {
        return new LineSplitEnumerator(content);
    }
#else
    // .NET Framework 4.8 fallback - return IEnumerable<string> instead of ref struct
    internal static IEnumerable<string> ReadLines(this string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            yield break;
        }

        int pos = 0;
        while (pos < content.Length)
        {
            int lineEnd = content.IndexOfAny(new[] { '\r', '\n' }, pos);
            
            if (lineEnd >= 0)
            {
                yield return content.Substring(pos, lineEnd - pos);
                
                // Handle \r\n
                if (content[lineEnd] == '\r' && lineEnd + 1 < content.Length && content[lineEnd + 1] == '\n')
                {
                    pos = lineEnd + 2;
                }
                else
                {
                    pos = lineEnd + 1;
                }
            }
            else
            {
                yield return content.Substring(pos);
                break;
            }
        }
    }
#endif

    internal static string Replace(this string content, StringReplaceValues replacements)
    {
        return replacements.Replace(content);
    }

    public static string FixMultipleNewlines(this string markdown)
    {
        var normalizedMarkdown = markdown.ReplaceLineEndings(Environment.NewLine);
        return Regex.Replace(normalizedMarkdown, $"{Environment.NewLine}{{2,}}", Environment.NewLine + Environment.NewLine);
    }

    /// <summary>
    /// Compacts HTML by removing line breaks and collapsing whitespace between HTML tags only,
    /// while preserving spaces within tag content. This is useful for nested tables in markdown.
    /// </summary>
    /// <param name="html">The HTML string to compact</param>
    /// <returns>Compacted HTML string suitable for embedding in markdown tables</returns>
    public static string CompactHtmlForMarkdown(this string html)
    {
        if (string.IsNullOrEmpty(html))
            return html;

        // First remove all line endings
        html = html.ReplaceLineEndings("");

        // Use regex to collapse multiple spaces between tags (>...< patterns)
        // This preserves spaces within tag content
        html = Regex.Replace(html, @">\s+<", "> <");

        // Also trim any leading/trailing whitespace
        return html.Trim();
    }
}
