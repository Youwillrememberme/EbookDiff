using System.Text.RegularExpressions;

namespace EbookDiff.Services;

/// <summary>
/// 把 Calibre 输出的纯文本归一化为段落数组。
/// 按空行切段，段内折叠连续空白，丢弃空段。
/// </summary>
public static class TextNormalizer
{
    private static readonly Regex BlankLineSplit = new(@"\r?\n\s*\r?\n", RegexOptions.Compiled);
    private static readonly Regex InnerWhitespace = new(@"\s+", RegexOptions.Compiled);

    /// <summary>
    /// 将原始文本切分为段落。
    /// </summary>
    public static string[] ToParagraphs(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return Array.Empty<string>();

        var parts = BlankLineSplit.Split(raw);
        var list = new List<string>(parts.Length);
        foreach (var p in parts)
        {
            var norm = InnerWhitespace.Replace(p, " ").Trim();
            if (norm.Length > 0)
                list.Add(norm);
        }
        return list.ToArray();
    }
}
