$ErrorActionPreference = 'Stop'
Set-Location -LiteralPath $PSScriptRoot
$env:ASPNETCORE_ENVIRONMENT = 'Development'
Write-Host 'Hearth & Handle: http://localhost:5188'
Write-Host 'Manager credentials: src/HearthCup.Web/.local/admin-login.txt (created on first start)'
dotnet run --project src/HearthCup.Web --no-launch-profile --urls http://localhost:5188
