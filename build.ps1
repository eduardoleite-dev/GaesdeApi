$ErrorActionPreference = "Stop"

$apiProject = Join-Path $PSScriptRoot "GaesdeApi.csproj"
$testProject = Join-Path $PSScriptRoot "Test\GaesdeApi.Tests.csproj"

function Invoke-Dotnet {
    param(
        [Parameter(Mandatory = $true)]
        [string[]] $Arguments
    )

    Write-Host "> dotnet $($Arguments -join ' ')" -ForegroundColor Cyan
    & dotnet @Arguments

    if ($LASTEXITCODE -ne 0) {
        throw "O comando dotnet falhou com o código $LASTEXITCODE."
    }
}

Write-Host "=== Clean ===" -ForegroundColor Yellow
Invoke-Dotnet @("clean", $apiProject)
Invoke-Dotnet @("clean", $testProject)

Write-Host "=== Restore ===" -ForegroundColor Yellow
Invoke-Dotnet @("restore", $apiProject)
Invoke-Dotnet @("restore", $testProject)

Write-Host "=== Build ===" -ForegroundColor Yellow
Invoke-Dotnet @("build", $apiProject, "--no-restore")
Invoke-Dotnet @("build", $testProject, "--no-restore")

Write-Host "=== Test ===" -ForegroundColor Yellow
Invoke-Dotnet @("test", $testProject, "--no-restore", "--no-build")

Write-Host "=== Pipeline concluído com sucesso ===" -ForegroundColor Green
