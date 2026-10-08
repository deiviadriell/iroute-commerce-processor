@echo off
chcp 65001 > nul
title Instalar base de datos IRouteCommerce

set "SERVER=.\SQLEXPRESS"
set /p "INPUT=Servidor SQL Server [.\SQLEXPRESS]: "
if not "%INPUT%"=="" set "SERVER=%INPUT%"

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0install.ps1" -Server "%SERVER%"

echo.
pause
