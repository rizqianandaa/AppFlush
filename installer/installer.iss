; Inno Setup script untuk AppFlush (versi C#/.NET WinForms).
; Dijalankan lewat GitHub Actions (lihat .github/workflows/build-windows.yml) --
; PublishDir diisi hasil "dotnet publish" self-contained single-file win-x64,
; jadi pengguna akhir TIDAK perlu install .NET Runtime terpisah.

#define MyAppName "AppFlush"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "AppFlush"
#define MyAppExeName "AppFlush.exe"
#ifndef PublishDir
  #define PublishDir "..\publish"
#endif

[Setup]
AppId={{B7B1E2B0-6E6B-4C2E-9B4E-APPFLUSHCSHARP}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputBaseFilename=AppFlush-Setup-{#MyAppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
UninstallDisplayIcon={app}\{#MyAppExeName}
SetupIconFile=..\src\AppFlush.App\Resources\AppFlush.ico
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Buat shortcut di Desktop"; GroupDescription: "Shortcut tambahan:"

[Run]
Filename: "{app}\{#MyAppExeName}"; Parameters: "register-menu"; Flags: runhidden; StatusMsg: "Mendaftarkan menu klik-kanan..."
Filename: "{app}\{#MyAppExeName}"; Description: "Buka {#MyAppName} sekarang"; Flags: nowait postinstall skipifsilent

[UninstallRun]
Filename: "{app}\{#MyAppExeName}"; Parameters: "unregister-menu"; Flags: runhidden; RunOnceId: "UnregisterAppFlushMenu"
