$AppName     = "AppManager"
$ProjectName = "AppManager"
$OutDir      = "dist"

$arch    = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture
$Runtime = if ($arch -eq [System.Runtime.InteropServices.Architecture]::Arm64) { "win-arm64" } else { "win-x64" }

Write-Host "Building $AppName for $Runtime..."

if (Test-Path $OutDir) { Remove-Item $OutDir -Recurse -Force }

dotnet publish -c Release -r $Runtime --self-contained true -p:PublishSingleFile=true -o $OutDir

Get-ChildItem $OutDir -Filter "*.pdb" | Remove-Item -Force

$exe = Join-Path (Resolve-Path $OutDir) "$ProjectName.exe"
Write-Host "Done! Executable: $exe"
