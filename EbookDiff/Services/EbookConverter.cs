using System.Diagnostics;
using System.IO;
using System.Text;

namespace EbookDiff.Services;

/// <summary>
/// 调用 Calibre 的 ebook-convert，把任意格式的电子书转成纯文本。
/// 跨格式对比的关键：两本书都经过同一转换路径，得到一致的纯文本表示。
/// </summary>
public static class EbookConverter
{
    private const int TimeoutMs = 120_000; // Calibre 启动较慢，给足时间

    /// <summary>
    /// 将电子书转换为 UTF-8 纯文本。
    /// </summary>
    /// <param name="ebookPath">电子书文件路径。</param>
    /// <param name="calibrePath">ebook-convert.exe 的完整路径。</param>
    /// <param name="progress">可选，用于报告阶段文本。</param>
    /// <returns>转换后的纯文本。</returns>
    /// <exception cref="InvalidOperationException">转换失败时抛出，附带 stderr。</exception>
    public static string ConvertToText(string ebookPath, string calibrePath, Action<string>? progress = null)
    {
        if (!File.Exists(ebookPath))
            throw new FileNotFoundException("电子书文件不存在：", ebookPath);

        // txt 源文件直接读取，无需转换
        if (Path.GetExtension(ebookPath).Equals(".txt", StringComparison.OrdinalIgnoreCase))
            return File.ReadAllText(ebookPath, Encoding.UTF8);

        var tempOutput = Path.Combine(Path.GetTempPath(), $"ebookdiff_{Guid.NewGuid():N}.txt");
        try
        {
            progress?.Invoke($"正在转换 {Path.GetFileName(ebookPath)} …");

            var psi = new ProcessStartInfo
            {
                FileName = calibrePath,
                Arguments = $"\"{ebookPath}\" \"{tempOutput}\" --txt-output-encoding=utf-8",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                StandardErrorEncoding = Encoding.UTF8,
                StandardOutputEncoding = Encoding.UTF8,
            };

            using var proc = new Process { StartInfo = psi };
            var stderr = new StringBuilder();
            proc.ErrorDataReceived += (_, e) => { if (e.Data != null) stderr.AppendLine(e.Data); };
            // stdout 不读会填满 pipe buffer 导致进程卡死，全量丢弃即可（真正想看的诊断信息在 stderr）。
            proc.OutputDataReceived += (_, _) => { };

            if (!proc.Start())
                throw new InvalidOperationException("无法启动 ebook-convert。");
            proc.BeginErrorReadLine();
            proc.BeginOutputReadLine();

            if (!proc.WaitForExit(TimeoutMs))
            {
                try { proc.Kill(true); } catch { /* 忽略 */ }
                throw new InvalidOperationException("ebook-convert 转换超时。");
            }

            if (proc.ExitCode != 0)
                throw new InvalidOperationException($"ebook-convert 转换失败（退出码 {proc.ExitCode}）：\n{stderr}");

            if (!File.Exists(tempOutput))
                throw new InvalidOperationException("转换完成但未生成输出文件。");

            return File.ReadAllText(tempOutput, Encoding.UTF8);
        }
        finally
        {
            try { if (File.Exists(tempOutput)) File.Delete(tempOutput); } catch { /* 忽略 */ }
        }
    }
}
