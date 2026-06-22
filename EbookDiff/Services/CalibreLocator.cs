using System.IO;
using System.Text.Json;

namespace EbookDiff.Services;

/// <summary>
/// 探测并持久化 Calibre 的 ebook-convert.exe 路径。
/// 设置保存在 %AppData%\EbookDiff\settings.json。
/// </summary>
public static class CalibreLocator
{
    private static readonly string SettingsDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EbookDiff");

    private static readonly string SettingsPath = Path.Combine(SettingsDir, "settings.json");

    /// <summary>构建常见 Calibre 安装位置候选列表（基于实际 ProgramFiles 环境变量）。</summary>
    private static IEnumerable<string> ProbePaths()
    {
        var roots = new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            Environment.GetEnvironmentVariable("ProgramW6432"),
            // 兜底 D 盘自定义安装位置（不少国内用户把软件装在 D 盘）
            @"D:\Program Files",
            @"D:\Program Files (x86)",
        };
        foreach (var root in roots)
        {
            if (string.IsNullOrWhiteSpace(root)) continue;
            yield return Path.Combine(root, "Calibre2", "ebook-convert.exe");
            yield return Path.Combine(root, "Calibre", "ebook-convert.exe");
        }
    }

    /// <summary>
    /// 获取当前可用的 ebook-convert 路径。优先用已保存的设置，其次探测 PATH 与常见安装位置。
    /// 找不到返回 null。
    /// </summary>
    public static string? Resolve()
    {
        // 1. 已保存的设置
        var saved = LoadSaved();
        if (!string.IsNullOrWhiteSpace(saved) && File.Exists(saved))
            return saved;

        // 2. PATH 中的 ebook-convert（不带扩展名时补 .exe）
        var pathVar = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var dir in pathVar.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            var candidate = Path.Combine(dir.Trim(), "ebook-convert.exe");
            if (File.Exists(candidate))
                return candidate;
        }

        // 3. 常见安装位置
        foreach (var p in ProbePaths())
            if (File.Exists(p))
                return p;

        return null;
    }

    /// <summary>保存用户手动指定的路径。</summary>
    public static void Save(string path)
    {
        Directory.CreateDirectory(SettingsDir);
        var json = JsonSerializer.Serialize(new { CalibrePath = path });
        File.WriteAllText(SettingsPath, json);
    }

    private static string? LoadSaved()
    {
        try
        {
            if (!File.Exists(SettingsPath))
                return null;
            using var doc = JsonDocument.Parse(File.ReadAllText(SettingsPath));
            return doc.RootElement.TryGetProperty("CalibrePath", out var v)
                ? v.GetString()
                : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>判定给定扩展名是否为受支持的电子书格式。</summary>
    public static bool IsSupportedFormat(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext is ".epub" or ".mobi" or ".azw" or ".azw3" or ".txt";
    }
}
