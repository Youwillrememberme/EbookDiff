using System.Windows.Media;

namespace EbookDiff.Models;

/// <summary>
/// 一行并排对齐的差异结果。左侧对应书籍 A，右侧对应书籍 B。
/// </summary>
public class DiffRow
{
    public DiffType Type { get; init; }

    /// <summary>书籍 A 的段落文本（Deleted/Equal 时有值，Added 时为空）。</summary>
    public string LeftText { get; init; } = string.Empty;

    /// <summary>书籍 B 的段落文本（Added/Equal 时有值，Deleted 时为空）。</summary>
    public string RightText { get; init; } = string.Empty;

    /// <summary>行首标记符号（等宽字体显示）。</summary>
    public string Marker => Type switch
    {
        DiffType.Equal => " ",
        DiffType.Deleted => "−",
        DiffType.Added => "+",
        _ => " ",
    };

    /// <summary>左侧单元格背景色。</summary>
    public Brush LeftBackground => Type == DiffType.Deleted
        ? Brushes.Black
        : Brushes.White;

    /// <summary>左侧文字颜色（黑底白字 / 白底黑字）。</summary>
    public Brush LeftForeground => Type == DiffType.Deleted
        ? Brushes.White
        : Brushes.Black;

    /// <summary>右侧单元格背景色。</summary>
    public Brush RightBackground => Type == DiffType.Added
        ? Brushes.Black
        : Brushes.White;

    /// <summary>右侧文字颜色。</summary>
    public Brush RightForeground => Type == DiffType.Added
        ? Brushes.White
        : Brushes.Black;

    /// <summary>行底分隔线颜色——差异行用黑色细线突出，普通行用浅灰。</summary>
    public Brush Divider => Type == DiffType.Equal
        ? new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0))
        : Brushes.Black;
}
