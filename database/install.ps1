param([string]$Server = ".\SQLEXPRESS")

$ErrorActionPreference = "Stop"

if (-not (Get-Command sqlcmd -ErrorAction SilentlyContinue)) {
    Write-Host "No se encontró sqlcmd. Instale las herramientas de línea de comandos de SQL Server (sqlcmd)." -ForegroundColor Red
    exit 1
}

Write-Host "Servidor: $Server" -ForegroundColor Cyan

sqlcmd -S $Server -E -C -b -Q "SELECT 1" | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "No fue posible conectar con $Server. Verifique que SQL Server esté iniciado y el nombre de la instancia." -ForegroundColor Red
    exit 1
}

foreach ($script in Get-ChildItem -Path $PSScriptRoot -Filter "0*.sql" | Sort-Object Name) {
    Write-Host "Ejecutando $($script.Name)"
    sqlcmd -S $Server -E -C -b -f 65001 -i $script.FullName | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Falló $($script.Name)" -ForegroundColor Red
        exit 1
    }
}

Write-Host "Base de datos IRouteCommerce instalada correctamente." -ForegroundColor Green
