param([string]$SdkVersion='1.0.4258.31')
$ErrorActionPreference='Stop'
$repoRoot=Split-Path -Parent $PSScriptRoot
$cacheRoot=Join-Path $repoRoot '.cache'
$sdkRoot=Join-Path $cacheRoot 'webview2'
$distRoot=Join-Path $repoRoot 'dist'
$appRoot=Join-Path $distRoot '닌자나월'
New-Item -ItemType Directory -Force -Path $cacheRoot,$appRoot | Out-Null
if(!(Test-Path (Join-Path $sdkRoot 'lib/net462/Microsoft.Web.WebView2.Core.dll'))){
 $packagePath=Join-Path $cacheRoot 'webview2.zip'
 Invoke-WebRequest -Uri "https://api.nuget.org/v3-flatcontainer/microsoft.web.webview2/$SdkVersion/microsoft.web.webview2.$SdkVersion.nupkg" -OutFile $packagePath
 Expand-Archive -LiteralPath $packagePath -DestinationPath $sdkRoot -Force
}
$core=Join-Path $sdkRoot 'lib/net462/Microsoft.Web.WebView2.Core.dll'
$forms=Join-Path $sdkRoot 'lib/net462/Microsoft.Web.WebView2.WinForms.dll'
Copy-Item -LiteralPath $core,$forms -Destination $appRoot -Force
Copy-Item -LiteralPath (Join-Path $sdkRoot 'runtimes/win-x64/native/WebView2Loader.dll') -Destination $appRoot -Force
Copy-Item -LiteralPath (Join-Path $sdkRoot 'LICENSE.txt') -Destination (Join-Path $appRoot 'WebView2-LICENSE.txt') -Force
Copy-Item -LiteralPath (Join-Path $sdkRoot 'NOTICE.txt') -Destination (Join-Path $appRoot 'WebView2-NOTICE.txt') -Force
Copy-Item -LiteralPath (Join-Path $repoRoot 'game') -Destination $appRoot -Recurse -Force
Copy-Item -LiteralPath (Join-Path $repoRoot 'README.md') -Destination (Join-Path $appRoot 'README.md') -Force
$compiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
& $compiler /nologo /target:winexe /platform:x64 "/win32icon:$(Join-Path $repoRoot 'game/assets/app-icon.ico')" "/out:$(Join-Path $appRoot '닌자나월.exe')" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Web.Extensions.dll "/reference:$core" "/reference:$forms" (Join-Path $PSScriptRoot 'Program.cs')
if($LASTEXITCODE -ne 0){throw 'Windows 앱 컴파일 실패'}
Compress-Archive -LiteralPath $appRoot -DestinationPath (Join-Path $distRoot '닌자나월-Windows.zip') -Force
Write-Host "빌드 완료: $appRoot"
