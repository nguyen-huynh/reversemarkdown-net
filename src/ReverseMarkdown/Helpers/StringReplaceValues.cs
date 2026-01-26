using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace ReverseMarkdown.Helpers;

internal class StringReplaceValues : Dictionary<string, string> {
    private Regex? _regex;

    private Regex Regex => _regex ??= new Regex($"{string.Join("|", this.Keys.Select(Regex.Escape))}", RegexOptions.Compiled);

    public string Replace(string input)
    {
#if NET5_0_OR_GREATER
        var offset = 0;
        StringBuilder? sb = null;
        foreach (var match in Regex.EnumerateMatches(input)) {
            sb ??= new StringBuilder(input.Length);
            sb.Append(input.AsSpan(offset, match.Index - offset));
            sb.Append(this[input.AsSpan(match.Index, match.Length).ToString()]);
            offset = match.Index + match.Length;
        }

        if (sb is not null && offset != input.Length) {
            sb.Append(input.AsSpan(offset, input.Length - offset));
        }

        return sb?.ToString() ?? input;
#else
        // .NET Framework 4.8 fallback - use regular Regex.Replace
        return Regex.Replace(input, match => this[match.Value]);
#endif
    }
}
