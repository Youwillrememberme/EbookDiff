using System.IO;
using System.Windows;
using System.Windows.Controls;
using EbookDiff.Models;
using EbookDiff.Services;
using Microsoft.Win32;

namespace EbookDiff;

/// <summary>
/// 主窗口：文件选择 → Calibre 转纯文本 → 切段 → 段落级 diff → 并排渲染。
/// 全程异步，转换期间禁用按钮并更新状态。
/// </summary>
public partial class MainWindow : Window
{
    private string? _pathA;
    private string? _pathB;
    private bool _busy;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void PickA_Click(object sender, RoutedEventArgs e) => PickFile(PathABox, p => _pathA = p);
    private void PickB_Click(object sender, RoutedEventArgs e) => PickFile(PathBBox, p => _pathB = p);

    private void PickFile(TextBox target, Action<string?> setter)
    {
        var dlg = new OpenFileDialog
        {
            Title = "选择电子书",
            Filter = "电子书|*.epub;*.mobi;*.azw;*.azw3;*.txt|所有文件|*.*",
            CheckFileExists = true,
        };
        if (dlg.ShowDialog() == true)
        {
            target.Text = dlg.FileName;
            setter(dlg.FileName);
            DiffList.ItemsSource = null;
            StatusText.Text = $"已选择：{Path.GetFileName(dlg.FileName)}";
        }
    }

    private async void Compare_Click(object sender, RoutedEventArgs e)
    {
        if (_busy) return;

        if (string.IsNullOrWhiteSpace(_pathA) || string.IsNullOrWhiteSpace(_pathB))
        {
            StatusText.Text = "请先选择两本电子书。";
            return;
        }

        if (string.Equals(_pathA, _pathB, StringComparison.OrdinalIgnoreCase))
        {
            StatusText.Text = "两本书是同一个文件，无需对比。";
            return;
        }

        // 定位 Calibre
        string? calibrePath = CalibreLocator.Resolve();
        if (calibrePath == null)
        {
            calibrePath = PromptForCalibre();
            if (calibrePath == null)
            {
                StatusText.Text = "未找到 Calibre。请先安装 Calibre，或手动指定 ebook-convert.exe 路径。";
                return;
            }
        }

        SetBusy(true);
        DiffList.ItemsSource = null;
        try
        {
            StatusText.Text = "正在转换书籍 A …";
            var textA = await Task.Run(() => EbookConverter.ConvertToText(_pathA!, calibrePath));

            StatusText.Text = "正在转换书籍 B …";
            var textB = await Task.Run(() => EbookConverter.ConvertToText(_pathB!, calibrePath));

            StatusText.Text = "正在比较差异 …";
            var paragraphsA = TextNormalizer.ToParagraphs(textA);
            var paragraphsB = TextNormalizer.ToParagraphs(textB);

            var rows = await Task.Run(() => ParagraphDiffer.Diff(paragraphsA, paragraphsB));
            DiffList.ItemsSource = rows;

            var diffCount = ParagraphDiffer.CountDifferences(rows);
            StatusText.Text = $"对比完成：书籍 A 共 {paragraphsA.Length} 段，书籍 B 共 {paragraphsB.Length} 段，差异 {diffCount} 处。";
        }
        catch (Exception ex)
        {
            StatusText.Text = "出错：" + ex.Message;
            MessageBox.Show(ex.Message, "对比失败", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            SetBusy(false);
        }
    }

    /// <summary>Calibre 未找到时，让用户手动定位 ebook-convert.exe。</summary>
    private string? PromptForCalibre()
    {
        var dlg = new OpenFileDialog
        {
            Title = "未自动找到 Calibre，请定位 ebook-convert.exe",
            Filter = "ebook-convert.exe|ebook-convert.exe",
            CheckFileExists = true,
        };
        if (dlg.ShowDialog() == true)
        {
            CalibreLocator.Save(dlg.FileName);
            return dlg.FileName;
        }
        return null;
    }

    private void SetBusy(bool busy)
    {
        _busy = busy;
        CompareBtn.IsEnabled = !busy;
        PickA.IsEnabled = !busy;
        PickB.IsEnabled = !busy;
        CompareBtn.Content = busy ? "处理中…" : "对  比";
        Cursor = busy ? System.Windows.Input.Cursors.Wait : System.Windows.Input.Cursors.Arrow;
    }
}
