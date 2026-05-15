using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ServisBinjaket.Application.Common;

public static class SlugHelper
{
    private static readonly Dictionary<char, string> AlbanianMap = new()
    {
        ['ë'] = "e", ['Ë'] = "e",
        ['ç'] = "c", ['Ç'] = "c",
    };

    public static string Generate(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "";

        var sb = new StringBuilder(input.Length);
        foreach (var ch in input)
        {
            if (AlbanianMap.TryGetValue(ch, out var replacement))
                sb.Append(replacement);
            else
                sb.Append(ch);
        }

        var s = sb.ToString().ToLowerInvariant();

        s = string.Concat(
            s.Normalize(NormalizationForm.FormD)
             .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark))
             .Normalize(NormalizationForm.FormC);

        s = Regex.Replace(s, @"[^a-z0-9\s\-]", "");
        s = Regex.Replace(s, @"[\s\-]+", "-");
        return s.Trim('-');
    }
}
