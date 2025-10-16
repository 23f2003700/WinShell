; WinShell Installer Script for Inno Setup
; This creates a single installer with both CLI and GUI versions

#define MyAppName "WinShell"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "WinShell Project"
#define MyAppURL "https://github.com/winshell/winshell"
#define MyAppExeName "WinShell.GUI.exe"
#define MyAppCLIExeName "WinShell.CLI.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
AppId={{8B5C9D2A-1234-5678-9ABC-DEF012345678}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=
InfoBeforeFile=
InfoAfterFile=
OutputDir=Installer
OutputBaseFilename=WinShell-{#MyAppVersion}-Setup
SetupIconFile=favicon.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
WizardImageFile=
WizardSmallImageFile=
ArchitecturesInstallIn64BitMode=x64
MinVersion=10.0.17763

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1; Check: not IsAdminInstallMode
Name: "addtopath"; Description: "Add WinShell CLI to PATH environment variable"; GroupDescription: "System Integration"; Flags: unchecked

[Files]
; GUI Application Files - Self-Contained with .NET Runtime
Source: "publish-temp\gui\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

; CLI Application Files - Self-Contained with .NET Runtime  
Source: "publish-temp\cli\*"; DestDir: "{app}\cli"; Flags: ignoreversion recursesubdirs createallsubdirs

; Icons and Assets
Source: "ok.png"; DestDir: "{app}"; Flags: ignoreversion
Source: "favicon.ico"; DestDir: "{app}"; Flags: ignoreversion

; Documentation
Source: "README.md"; DestDir: "{app}"; Flags: ignoreversion isreadme

; Launch Scripts
Source: "launch-cli.ps1"; DestDir: "{app}"; Flags: ignoreversion
Source: "launch-cli.bat"; DestDir: "{app}"; Flags: ignoreversion

[Registry]
; Add CLI to PATH if selected (now points to cli subfolder)
Root: HKLM; Subkey: "SYSTEM\CurrentControlSet\Control\Session Manager\Environment"; ValueType: expandsz; ValueName: "Path"; ValueData: "{olddata};{app}\cli"; Tasks: addtopath; Check: NeedsAddPath('{app}\cli')

[Icons]
; Start Menu Icons
Name: "{group}\{#MyAppName} GUI"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\favicon.ico"
Name: "{group}\{#MyAppName} CLI"; Filename: "{app}\cli\{#MyAppCLIExeName}"; IconFilename: "{app}\favicon.ico"
Name: "{group}\Launch CLI (PowerShell)"; Filename: "powershell.exe"; Parameters: "-ExecutionPolicy Bypass -File ""{app}\launch-cli.ps1"""; IconFilename: "{app}\favicon.ico"
Name: "{group}\Launch CLI (Batch)"; Filename: "{app}\launch-cli.bat"; IconFilename: "{app}\favicon.ico"
Name: "{group}\{cm:ProgramOnTheWeb,{#MyAppName}}"; Filename: "{#MyAppURL}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"

; Desktop Icons (optional)
Name: "{autodesktop}\{#MyAppName} GUI"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\favicon.ico"; Tasks: desktopicon
Name: "{autodesktop}\{#MyAppName} CLI"; Filename: "{app}\cli\{#MyAppCLIExeName}"; IconFilename: "{app}\favicon.ico"; Tasks: desktopicon

; Quick Launch Icons (optional)
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName} GUI"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\favicon.ico"; Tasks: quicklaunchicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName} CLI"; Filename: "{app}\cli\{#MyAppCLIExeName}"; IconFilename: "{app}\favicon.ico"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')} GUI}"; Flags: nowait postinstall skipifsilent
Filename: "{app}\cli\{#MyAppCLIExeName}"; Description: "Launch {#StringChange(MyAppName, '&', '&&')} CLI"; Flags: nowait postinstall skipifsilent unchecked

[UninstallDelete]
Type: filesandordirs; Name: "{app}"

[Code]
function NeedsAddPath(Param: string): boolean;
var
  OrigPath: string;
begin
  if not RegQueryStringValue(HKEY_LOCAL_MACHINE,
    'SYSTEM\CurrentControlSet\Control\Session Manager\Environment',
    'Path', OrigPath)
  then begin
    Result := True;
    exit;
  end;
  { look for the path with leading and trailing semicolon }
  { Pos() returns 0 if not found }
  Result := Pos(';' + Param + ';', ';' + OrigPath + ';') = 0;
end;

procedure InitializeWizard;
begin
  WizardForm.WelcomeLabel1.Caption := 'Welcome to the WinShell Setup Wizard';
  WizardForm.WelcomeLabel2.Caption := 'This will install WinShell v' + '{#MyAppVersion}' + ' on your computer.' + #13#10 + #13#10 +
    'WinShell is an advanced Windows shell with both GUI and CLI interfaces.' + #13#10 + #13#10 +
    'Features:' + #13#10 +
    '• Modern GUI Terminal Interface' + #13#10 +
    '• Standalone CLI Terminal' + #13#10 +
    '• Advanced Shell Commands' + #13#10 +
    '• Customizable Prompts' + #13#10 +
    '• PowerShell Integration' + #13#10 + #13#10 +
    'Click Next to continue, or Cancel to exit Setup.';
end;