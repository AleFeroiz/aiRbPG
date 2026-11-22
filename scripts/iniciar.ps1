$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$env:AIRBPG_SQL_CONNECTION = [Environment]::GetEnvironmentVariable('AIRBPG_SQL_CONNECTION','User')
if ([string]::IsNullOrWhiteSpace($env:AIRBPG_SQL_CONNECTION)) {
    throw 'Conexao ausente. Execute scripts\configurar-banco.ps1 antes de iniciar.'
}
$exe = Join-Path $root 'src\aiRbPG.WinForms\bin\Release\aiRbPG.WinForms.exe'
if (!(Test-Path $exe)) { throw 'Compile a solucao em Release antes de iniciar.' }
Add-Type -AssemblyName System.Data
$connection = New-Object System.Data.SqlClient.SqlConnection $env:AIRBPG_SQL_CONNECTION
try {
    $connection.Open()
    $command = $connection.CreateCommand()
    $command.CommandText = "SELECT CASE WHEN OBJECT_ID(N'dbo.HistoricoReservas',N'U') IS NULL THEN 0 ELSE 1 END;"
    if ($command.ExecuteScalar() -ne 1) { throw 'Banco desatualizado. Execute scripts\configurar-banco.ps1.' }
} finally { $connection.Dispose() }
Start-Process -FilePath $exe -WorkingDirectory $root
