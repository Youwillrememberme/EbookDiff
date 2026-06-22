namespace EbookDiff.Models;

/// <summary>
/// 一行差异的类型。
/// </summary>
public enum DiffType
{
    /// <summary>两本书中都有且相同。</summary>
    Equal,

    /// <summary>仅在书籍 A 中存在（旧版本内容，相当于“删除”）。</summary>
    Deleted,

    /// <summary>仅在书籍 B 中存在（新版本内容，相当于“新增”）。</summary>
    Added,
}
