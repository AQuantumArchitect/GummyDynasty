# Agent-runnable. Does not need Unity Hub or a license handshake.
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
if (-not (Test-Path (Join-Path $root "README.md"))) { $root = $PSScriptRoot }
$csc = "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csc.exe"
if (-not (Test-Path $csc)) {
    $csc = "C:\Program Files\Microsoft Visual Studio\2022\Preview\MSBuild\Current\Bin\Roslyn\csc.exe"
}
if (-not (Test-Path $csc)) { throw "csc.exe not found (need VS2022 Roslyn)" }

$outDir = Join-Path $root "Tools\Harness\bin"
New-Item -ItemType Directory -Force -Path $outDir | Out-Null
$exe = Join-Path $outDir "harness.exe"

$sources = @()
$sources += Get-ChildItem (Join-Path $root "Tools\Harness\*.cs") | ForEach-Object { $_.FullName }
$sources += Get-ChildItem (Join-Path $root "Assets\_Project\Runtime\Cognition\*.cs") | ForEach-Object { $_.FullName }
$sources += Join-Path $root "Assets\_Project\Runtime\Core\QrEncode.cs"
$sources += Join-Path $root "Assets\_Project\Runtime\Simulation\LogicalSoldier.cs"
$sources += Join-Path $root "Assets\_Project\Runtime\Simulation\LogicalPopulation.cs"
$sources += Join-Path $root "Assets\_Project\Runtime\Simulation\MachineControls.cs"
$sources += Join-Path $root "Assets\_Project\Runtime\Simulation\PhoneCommand.cs"
$sources += Join-Path $root "Assets\_Project\Runtime\Simulation\PhoneSession.cs"
$sources += Join-Path $root "Assets\_Project\Runtime\Simulation\PhonePages.cs"
$sources += Join-Path $root "Assets\_Project\Runtime\Simulation\PhoneHost.cs"
$sources += Join-Path $root "Assets\_Project\Runtime\Simulation\GameModeRules.cs"
$sources += Join-Path $root "Assets\_Project\Runtime\Simulation\MarchFormation.cs"
$sources += Join-Path $root "Assets\_Project\Runtime\Simulation\WallMeasure.cs"

& $csc /nologo /optimize+ /langversion:latest /t:exe /out:$exe $sources
if ($LASTEXITCODE -ne 0) { throw "harness compile failed" }
& $exe
exit $LASTEXITCODE
