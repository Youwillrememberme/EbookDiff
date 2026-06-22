; EbookDiff Inno Setup 脚本
; 用法：装好 Inno Setup 6 后，双击此文件打开 → Build → 输出在 dist\ 目录
;
; 前置：先跑 build.bat 生成 EbookDiff\EbookDiff\bin\Release\net8.0-windows\win-x64\publish\EbookDiff.exe

#define MyAppName        "EbookDiff"
#define MyAppVersion     "1.0.0"
#define MyAppPublisher   "EbookDiff"
#define MyAppExeName     "EbookDiff.exe"
#define MyAppDescription "电子书版本对比工具"

#define PublishDir       "EbookDiff\bin\Release\net8.0-windows\win-x64\publish"

[Setup]
AppId={{B7A1C2D3-E4F5-4A6B-9C7D-8E9F0A1B2C3D}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL=https://github.com/
VersionInfoVersion={#MyAppVersion}
VersionInfoDescription={#MyAppDescription}

DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
DisableDirPage=no
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName} {#MyAppVersion}

OutputDir=dist
OutputBaseFilename={#MyAppName}-Setup-{#MyAppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
SetupIconFile=EbookDiff\Assets\app.ico

PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog

ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

MinVersion=10.0.17763

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#PublishDir}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "README.md"; DestDir: "{app}"; Flags: ignoreversion isreadme

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\{#MyAppExeName}"
Name: "{group}\卸载 {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#MyAppName}}"; Flags: nowait postinstall skipifsilent

[Code]
function CalibreInstalled(): Boolean;
var
  Roots: array of String;
  I: Integer;
begin
  SetArrayLength(Roots, 4);
  Roots[0] := ExpandConstant('{commonpf}');
  Roots[1] := ExpandConstant('{commonpf32}');
  Roots[2] := 'D:\Program Files';
  Roots[3] := 'D:\Program Files (x86)';

  Result := False;
  for I := 0 to GetArrayLength(Roots) - 1 do
  begin
    if FileExists(Roots[I] + '\Calibre2\ebook-convert.exe') or
       FileExists(Roots[I] + '\Calibre\ebook-convert.exe') then
    begin
      Result := True;
      Exit;
    end;
  end;
end;

function InitializeSetup(): Boolean;
var
  Response: Integer;
begin
  Result := True;
  if not CalibreInstalled() then
  begin
    Response := MsgBox(
      '未检测到 Calibre。' + #13#10 +
      'EbookDiff 依赖 Calibre 的 ebook-convert 工具来转换电子书格式。' + #13#10 + #13#10 +
      '建议先到 https://calibre-ebook.com/download 安装 Calibre，' + #13#10 +
      '安装完成后再运行本安装程序。' + #13#10 + #13#10 +
      '是否仍然继续安装 EbookDiff？（首次使用时仍需 Calibre）',
      mbConfirmation, MB_YESNO);
    if Response = IDNO then
      Result := False;
  end;
end;
