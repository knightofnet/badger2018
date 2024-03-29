; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "Badger2018"
#define MyAppVersionName = "DianumDublin"
#define MyAppVersion "1.3.0926.2125"
#define MyAppPublisher "WolfAryx informatique"
#define MyAppExeName "Badger2018.exe"

#define SubAppDir "{app}\apps"
#define SubFfDir "{app}\ff"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{E0AF0C49-FD19-4171-8D76-16FB94CCAA3A}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppVersionName}
AppPublisher={#MyAppPublisher}
UninstallDisplayName={#MyAppName} {#MyAppVersionName}
DefaultDirName={%USERPROFILE}\{#MyAppName}
DisableDirPage=yes
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
; Remove the following line to run in administrative install mode (install for all users.)
PrivilegesRequired=lowest
OutputDir=E:\CSharp\Donn�es accessoires\Badger\Inno
OutputBaseFilename=Badger2018
SetupIconFile=C:\Users\ARyx\Documents\Visual Studio 2013\Projects\Badger2018\Badger2018\Badger2018\Resources\Paomedia-Small-N-Flat-Clock.ico
Password=dsn
Encryption=yes
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "french"; MessagesFile: "compiler:Languages\French.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "C:\Users\ARyx\Documents\Visual Studio 2013\Projects\Badger2018\Badger2018\Badger2018\bin\Release\{#MyAppExeName}"; DestDir: "{#SubAppDir}"; Flags: ignoreversion
Source: "C:\Users\ARyx\Documents\Visual Studio 2013\Projects\Badger2018\Badger2018\Badger2018\bin\Release\AryxDevLibrary.dll"; DestDir: "{#SubAppDir}"; Flags: ignoreversion
Source: "C:\Users\ARyx\Documents\Visual Studio 2013\Projects\Badger2018\Badger2018\Badger2018\bin\Release\AryxDevViewLibrary.dll"; DestDir: "{#SubAppDir}"; Flags: ignoreversion
Source: "C:\Users\ARyx\Documents\Visual Studio 2013\Projects\Badger2018\Badger2018\Badger2018\bin\Release\AudioSwitcher.AudioApi.CoreAudio.dll"; DestDir: "{#SubAppDir}"; Flags: ignoreversion
Source: "C:\Users\ARyx\Documents\Visual Studio 2013\Projects\Badger2018\Badger2018\Badger2018\bin\Release\AudioSwitcher.AudioApi.dll"; DestDir: "{#SubAppDir}"; Flags: ignoreversion
Source: "C:\Users\ARyx\Documents\Visual Studio 2013\Projects\Badger2018\Badger2018\Badger2018\bin\Release\BadgerCommonLibrary.dll"; DestDir: "{#SubAppDir}"; Flags: ignoreversion
Source: "C:\Users\ARyx\Documents\Visual Studio 2013\Projects\Badger2018\Badger2018\Badger2018\bin\Release\BadgerPluginExtender.dll"; DestDir: "{#SubAppDir}"; Flags: ignoreversion
Source: "C:\Users\ARyx\Documents\Visual Studio 2013\Projects\Badger2018\Badger2018\Badger2018\bin\Release\geckodriver.exe"; DestDir: "{#SubAppDir}"; Flags: ignoreversion
Source: "C:\Users\ARyx\Documents\Visual Studio 2013\Projects\Badger2018\Badger2018\Badger2018\bin\Release\geckoWithLog.cmd"; DestDir: "{#SubAppDir}"; Flags: ignoreversion
Source: "C:\Users\ARyx\Documents\Visual Studio 2013\Projects\Badger2018\Badger2018\Badger2018\bin\Release\Interop.IWshRuntimeLibrary.dll"; DestDir: "{#SubAppDir}"; Flags: ignoreversion
Source: "C:\Users\ARyx\Documents\Visual Studio 2013\Projects\Badger2018\Badger2018\Badger2018\bin\Release\Ionic.Zip.Reduced.dll"; DestDir: "{#SubAppDir}"; Flags: ignoreversion
Source: "C:\Users\ARyx\Documents\Visual Studio 2013\Projects\Badger2018\Badger2018\Badger2018\bin\Release\Microsoft.Threading.Tasks.dll"; DestDir: "{#SubAppDir}"; Flags: ignoreversion
Source: "C:\Users\ARyx\Documents\Visual Studio 2013\Projects\Badger2018\Badger2018\Badger2018\bin\Release\Microsoft.Threading.Tasks.Extensions.Desktop.dll"; DestDir: "{#SubAppDir}"; Flags: ignoreversion
Source: "C:\Users\ARyx\Documents\Visual Studio 2013\Projects\Badger2018\Badger2018\Badger2018\bin\Release\Microsoft.Threading.Tasks.Extensions.dll"; DestDir: "{#SubAppDir}"; Flags: ignoreversion
Source: "C:\Users\ARyx\Documents\Visual Studio 2013\Projects\Badger2018\Badger2018\Badger2018\bin\Release\NAudio.dll"; DestDir: "{#SubAppDir}"; Flags: ignoreversion
Source: "C:\Users\ARyx\Documents\Visual Studio 2013\Projects\Badger2018\Badger2018\Badger2018\bin\Release\System.Data.SQLite.dll"; DestDir: "{#SubAppDir}"; Flags: ignoreversion
Source: "C:\Users\ARyx\Documents\Visual Studio 2013\Projects\Badger2018\Badger2018\Badger2018\bin\Release\System.IO.dll"; DestDir: "{#SubAppDir}"; Flags: ignoreversion
Source: "C:\Users\ARyx\Documents\Visual Studio 2013\Projects\Badger2018\Badger2018\Badger2018\bin\Release\System.Runtime.dll"; DestDir: "{#SubAppDir}"; Flags: ignoreversion
Source: "C:\Users\ARyx\Documents\Visual Studio 2013\Projects\Badger2018\Badger2018\Badger2018\bin\Release\System.Threading.Tasks.dll"; DestDir: "{#SubAppDir}"; Flags: ignoreversion
Source: "C:\Users\ARyx\Documents\Visual Studio 2013\Projects\Badger2018\Badger2018\Badger2018\bin\Release\WaveCompagnonPlayer.exe"; DestDir: "{#SubAppDir}"; Flags: ignoreversion
Source: "C:\Users\ARyx\Documents\Visual Studio 2013\Projects\Badger2018\Badger2018\Badger2018\bin\Release\WebDriver.dll"; DestDir: "{#SubAppDir}"; Flags: ignoreversion
Source: "C:\Users\ARyx\Documents\Visual Studio 2013\Projects\Badger2018\Badger2018\Badger2018\bin\Release\Resources\*"; DestDir: "{#SubAppDir}\Resources"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "C:\Users\ARyx\Documents\Visual Studio 2013\Projects\Badger2018\Badger2018\Badger2018\bin\Release\x64\*"; DestDir: "{#SubAppDir}\x64"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "C:\Users\ARyx\Documents\Visual Studio 2013\Projects\Badger2018\Badger2018\Badger2018\bin\Release\x86\*"; DestDir: "{#SubAppDir}\x86"; Flags: ignoreversion recursesubdirs createallsubdirs

[Files]
Source: "{code:GetLicensePath}"; DestDir: "{#SubAppDir}"; Flags: external



; Source: "C:\Users\ARyx\Documents\Visual Studio 2013\Projects\Badger2018\Badger2018\Badger2018\bin\Release\screenshots\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files                                                                                               

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{#SubAppDir}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{#SubAppDir}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{#SubAppDir}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent



[Code]

var
  LicenseFilePage: TInputFileWizardPage;

procedure InitializeWizard();
begin
  LicenseFilePage :=
    CreateInputFilePage(
      wpSelectDir,
      'S�lectionnez le chemin du fichier de licence',
      'O� est situ� le fichier de licence ?',
      'S�lectionnez le fichier de licence, puis cliquez sur Suivant.');

  LicenseFilePage.Add(
    'Fichier de licence:',         
    'Fichier de licence|*.auth|All files|*.*', 
    '.auth');       
                             
end;


function GetLicensePath(Param: string): string;
begin
  if Assigned(LicenseFilePage) then  
    Result := LicenseFilePage.Values[0];
end;


function NextButtonClick(CurPageID: Integer): Boolean;
begin
  if CurPageID = LicenseFilePage.ID then begin
      if GetLicensePath('') = '' then begin
          Result := False
      end
      else 
      begin
          Result := True      
      end
  end
  else  
    Result := True
end;