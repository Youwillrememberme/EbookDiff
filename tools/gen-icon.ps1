Add-Type -AssemblyName System.Drawing

# Output path passed from environment (caller resolves Chinese path)
$outputPath = $env:ICON_OUT
if (-not $outputPath) { throw "ICON_OUT environment variable not set" }

$sizes = @(16, 24, 32, 48, 64, 128, 256)
$bitmaps = @()

foreach ($size in $sizes) {
    $bmp = New-Object System.Drawing.Bitmap $size, $size, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit

    $g.Clear([System.Drawing.Color]::Black)

    $borderPen = New-Object System.Drawing.Pen([System.Drawing.Color]::White), 1
    $g.DrawRectangle($borderPen, 0, 0, $size - 1, $size - 1)
    $borderPen.Dispose()

    $text = "EB"
    $fontSize = [Math]::Max(6, [int]($size * 0.45))
    $font = New-Object System.Drawing.Font("Segoe UI", $fontSize, [System.Drawing.FontStyle]::Bold, [System.Drawing.GraphicsUnit]::Pixel)
    $brush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
    $sf = New-Object System.Drawing.StringFormat
    $sf.Alignment = [System.Drawing.StringAlignment]::Center
    $sf.LineAlignment = [System.Drawing.StringAlignment]::Center
    $rect = New-Object System.Drawing.RectangleF(0, 0, $size, $size)
    $g.DrawString($text, $font, $brush, $rect, $sf)

    $font.Dispose()
    $brush.Dispose()
    $sf.Dispose()
    $g.Dispose()

    $bitmaps += ,@($size, $bmp)
}

$ms = New-Object System.IO.MemoryStream
$bw = New-Object System.IO.BinaryWriter $ms

$bw.Write([UInt16]0)
$bw.Write([UInt16]1)
$bw.Write([UInt16]$bitmaps.Count)

$pngBytesList = @()
$headerSize = 6 + ($bitmaps.Count * 16)
$currentOffset = $headerSize

foreach ($pair in $bitmaps) {
    $size = $pair[0]
    $bmp = $pair[1]

    $pngStream = New-Object System.IO.MemoryStream
    $bmp.Save($pngStream, [System.Drawing.Imaging.ImageFormat]::Png)
    $pngBytes = $pngStream.ToArray()
    $pngStream.Dispose()
    $pngBytesList += ,$pngBytes

    $w = if ($size -ge 256) { 0 } else { $size }
    $h = if ($size -ge 256) { 0 } else { $size }
    $bw.Write([Byte]$w)
    $bw.Write([Byte]$h)
    $bw.Write([Byte]0)
    $bw.Write([Byte]0)
    $bw.Write([UInt16]1)
    $bw.Write([UInt16]32)
    $bw.Write([UInt32]$pngBytes.Length)
    $bw.Write([UInt32]$currentOffset)
    $currentOffset += $pngBytes.Length

    $bmp.Dispose()
}

foreach ($pngBytes in $pngBytesList) {
    $bw.Write($pngBytes)
}

[System.IO.File]::WriteAllBytes($outputPath, $ms.ToArray())
$bw.Dispose()
$ms.Dispose()

Write-Host ("Icon generated: {0} ({1} bytes)" -f $outputPath, (Get-Item $outputPath).Length)
