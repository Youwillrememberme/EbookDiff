# EbookDiff
To compare difference between difeferent editions of ebooks!

# EbookDiff · 电子书版本对比工具

极简的 Windows 桌面软件，比较两本电子书（可能是同一本书的不同版本）的内容差异。
支持 **EPUB / MOBI / AZW3 / TXT** 跨格式互比——两本书都经 Calibre 统一转成纯文本再对比，用户无需手动转换。
界面纯黑白，段落/行级差异（Git diff 风格）。

## 用户使用

### 安装

1. 安装 **Calibre**（提供 `ebook-convert` 转换工具）：<https://calibre-ebook.com/download>
2. 双击 `EbookDiff-Setup-1.0.0.exe`，按向导安装。
   - 安装程序会自动检查是否装了 Calibre，没装会提示。
   - 安装包内置 .NET 8 运行时，**用户不需要单独装 .NET**。

### 使用

1. 启动后点「选择…」分别选两本电子书（A、B）。
2. 点「对比」。两本书会先后被 Calibre 转成纯文本、切段、做段落级 LCS 比对。
3. 主区并排显示：左侧书籍 A、右侧书籍 B。
   - 行首 ` `（空格）= 两书相同
   - 行首 `−` = 仅 A 有（旧版本删除的内容），该行左栏黑底白字
   - 行首 `+` = 仅 B 有（新版本新增的内容），该行右栏黑底白字
4. 状态条显示段落数与差异处数。
