using EbookDiff.Models;

namespace EbookDiff.Services;

/// <summary>
/// 基于最长公共子序列（LCS）的段落级 diff。
/// 把两本书的段落数组对齐为 Equal / Deleted / Added 序列。
/// </summary>
public static class ParagraphDiffer
{
    /// <summary>
    /// 计算两段段落数组的差异。
    /// </summary>
    /// <param name="a">书籍 A 的段落数组（旧版本）。</param>
    /// <param name="b">书籍 B 的段落数组（新版本）。</param>
    /// <returns>逐行对齐的差异列表，左右两列可直接并排渲染。</returns>
    public static List<DiffRow> Diff(IReadOnlyList<string> a, IReadOnlyList<string> b)
    {
        var n = a.Count;
        var m = b.Count;

        // dp[i,j] = a[0..i) 与 b[0..j) 的 LCS 长度
        var dp = new int[n + 1, m + 1];
        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                if (a[i - 1] == b[j - 1])
                    dp[i, j] = dp[i - 1, j - 1] + 1;
                else
                    dp[i, j] = Math.Max(dp[i - 1, j], dp[i, j - 1]);
            }
        }

        // 回溯生成操作序列（从后往前，最后反转）
        var ops = new List<DiffRow>();
        int ii = n, jj = m;
        while (ii > 0 && jj > 0)
        {
            if (a[ii - 1] == b[jj - 1])
            {
                ops.Add(new DiffRow { Type = DiffType.Equal, LeftText = a[ii - 1], RightText = b[jj - 1] });
                ii--; jj--;
            }
            else if (dp[ii - 1, jj] >= dp[ii, jj - 1])
            {
                // a 中的段落被“删除”
                ops.Add(new DiffRow { Type = DiffType.Deleted, LeftText = a[ii - 1], RightText = string.Empty });
                ii--;
            }
            else
            {
                // b 中的段落是“新增”
                ops.Add(new DiffRow { Type = DiffType.Added, LeftText = string.Empty, RightText = b[jj - 1] });
                jj--;
            }
        }
        while (ii > 0)
        {
            ops.Add(new DiffRow { Type = DiffType.Deleted, LeftText = a[ii - 1], RightText = string.Empty });
            ii--;
        }
        while (jj > 0)
        {
            ops.Add(new DiffRow { Type = DiffType.Added, LeftText = string.Empty, RightText = b[jj - 1] });
            jj--;
        }

        ops.Reverse();
        return ops;
    }

    /// <summary>统计差异行数（Deleted + Added）。</summary>
    public static int CountDifferences(IReadOnlyList<DiffRow> rows)
    {
        var count = 0;
        foreach (var r in rows)
            if (r.Type != DiffType.Equal) count++;
        return count;
    }
}
