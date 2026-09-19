$ErrorActionPreference = 'Stop'

$Here = Split-Path -Parent $MyInvocation.MyCommand.Path
$Source = Join-Path $Here 'LocalAppControlCenter.cs'
$Output = Join-Path $Here 'LocalAppControlCenter.exe'

$candidates = @(
    "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe",
    "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\csc.exe"
)

$csc = $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1

if (-not $csc) {
    Write-Host ''
    Write-Host 'Windows C# compiler was not found.' -ForegroundColor Red
    Write-Host 'Install/enable .NET Framework 4.x, then run this builder again.'
    Read-Host 'Press Enter to close'
    exit 1
}

if (Test-Path $Output) {
    Remove-Item $Output -Force
}

Write-Host 'Building LocalAppControlCenter.exe ...' -ForegroundColor Cyan

& $csc `
    /nologo `
    /codepage:65001 `
    /target:winexe `
    /platform:anycpu `
    /optimize+ `
    /out:"$Output" `
    /reference:System.dll `
    /reference:System.Drawing.dll `
    /reference:System.Windows.Forms.dll `
    "$Source"

if ($LASTEXITCODE -ne 0 -or -not (Test-Path $Output)) {
    throw 'Compilation failed.'
}

Write-Host ''
Write-Host 'SUCCESS' -ForegroundColor Green
Write-Host "Created: $Output"
Write-Host ''
Write-Host 'You can now double-click LocalAppControlCenter.exe.'
Write-Host 'The EXE saves its settings beside itself in controlcenter.ini.'
Write-Host ''
if (-not $env:CI) {
    Read-Host 'Press Enter to close'
}
