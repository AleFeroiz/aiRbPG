param([string]$Servidor = 'localhost\SQLEXPRESS', [ValidateSet('Debug','Release')][string]$Configuracao = 'Debug')
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$msbuild = Join-Path ${env:ProgramFiles(x86)} 'MSBuild\14.0\Bin\MSBuild.exe'
if (!(Test-Path $msbuild)) { throw 'Instale Microsoft Build Tools 2015 ou Visual Studio 2015.' }
& $msbuild (Join-Path $root 'aiRbPG.sln') /t:Rebuild "/p:Configuration=$Configuracao" '/p:Platform=Any CPU' /verbosity:minimal /nologo
if ($LASTEXITCODE -ne 0) { throw 'Falha no build da aplicacao.' }
& $msbuild (Join-Path $root 'tests\aiRbPG.Tests.csproj') /t:Rebuild "/p:Configuration=$Configuracao" /verbosity:minimal /nologo
if ($LASTEXITCODE -ne 0) { throw 'Falha no build dos testes.' }
Add-Type -AssemblyName System.Data
$builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
$builder['Data Source'] = $Servidor; $builder['Initial Catalog'] = 'master'; $builder['Integrated Security'] = $true; $builder['TrustServerCertificate'] = $true
$master = New-Object System.Data.SqlClient.SqlConnection $builder.ConnectionString
$testDb = 'aiRbPG_Test_' + [Guid]::NewGuid().ToString('N')
$created = $false
$oldConnection = $env:AIRBPG_SQL_CONNECTION
try {
    $master.Open()
    $command = $master.CreateCommand()
    $command.CommandText = "CREATE DATABASE [$testDb];"; $command.CommandTimeout = 120; [void]$command.ExecuteNonQuery(); $created = $true
    foreach ($file in Get-ChildItem (Join-Path $root 'database\scripts') -Filter '*.sql' | Sort-Object Name) {
        $sql = (Get-Content $file.FullName -Raw -Encoding UTF8).Replace('aiRbPG',$testDb)
        foreach ($batch in [regex]::Split($sql,'(?im)^\s*GO\s*\r?$')) {
            if ([string]::IsNullOrWhiteSpace($batch)) { continue }
            $command.CommandText = $batch; [void]$command.ExecuteNonQuery()
        }
    }
    $builder['Initial Catalog'] = $testDb; $env:AIRBPG_SQL_CONNECTION = $builder.ConnectionString
    & (Join-Path $root "tests\bin\$Configuracao\aiRbPG.Tests.exe")
    if ($LASTEXITCODE -ne 0) { throw 'Verificacoes funcionais falharam.' }
}
finally {
    $env:AIRBPG_SQL_CONNECTION = $oldConnection
    if ($created -and $testDb -match '^aiRbPG_Test_[0-9a-f]{32}$') {
        $command.CommandText = "USE master; ALTER DATABASE [$testDb] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$testDb];"
        [void]$command.ExecuteNonQuery(); Write-Host 'Banco temporario removido.'
    }
    $master.Dispose()
}
