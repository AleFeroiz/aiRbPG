param([string]$Servidor = 'localhost\SQLEXPRESS')
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Data
$builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
$builder["Data Source"] = $Servidor
$builder["Initial Catalog"] = 'master'
$builder["Integrated Security"] = $true
$builder["TrustServerCertificate"] = $true
$builder["Connect Timeout"] = 15
$connection = New-Object System.Data.SqlClient.SqlConnection $builder.ConnectionString
try {
    $connection.Open()
    $sql = (Get-ChildItem (Join-Path $PSScriptRoot '..\database\scripts') -Filter '*.sql' | Sort-Object Name | ForEach-Object { Get-Content $_.FullName -Raw -Encoding UTF8 }) -join "`r`nGO`r`n"
    foreach ($batch in [regex]::Split($sql, '(?im)^\s*GO\s*\r?$')) {
        if ([string]::IsNullOrWhiteSpace($batch)) { continue }
        $command = $connection.CreateCommand()
        try { $command.CommandText = $batch; $command.CommandTimeout = 120; [void]$command.ExecuteNonQuery() }
        finally { $command.Dispose() }
    }
    $builder["Initial Catalog"] = 'aiRbPG'
    [Environment]::SetEnvironmentVariable('AIRBPG_SQL_CONNECTION', $builder.ConnectionString, 'User')
    $env:AIRBPG_SQL_CONNECTION = $builder.ConnectionString
    Write-Host 'Banco aiRbPG preparado. Conexao salva para o usuario atual.'
    Write-Host 'Reabra o Visual Studio para carregar a variavel de ambiente.'
}
finally { $connection.Dispose() }
