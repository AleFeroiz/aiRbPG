$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$bin = Join-Path $root 'tests\bin\Release'
Copy-Item (Join-Path $root 'src\aiRbPG.WinForms\bin\Release\aiRbPG.WinForms.exe') $bin -Force
$compiler = Join-Path ${env:ProgramFiles(x86)} 'MSBuild\14.0\Bin\csc.exe'
& $compiler /nologo /langversion:6 /target:winexe "/out:$bin\aiRbPG.Interface.exe" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Data.dll "/reference:$bin\aiRbPG.Domain.dll" "/reference:$bin\aiRbPG.Data.dll" "/reference:$bin\aiRbPG.WinForms.exe" (Join-Path $root 'tests\InterfaceHarness.cs')
if ($LASTEXITCODE -ne 0) { throw 'Falha ao compilar entrada de QA.' }
Add-Type -AssemblyName System.Data
$testDb = 'aiRbPG_UI_' + [Guid]::NewGuid().ToString('N')
$connection = New-Object System.Data.SqlClient.SqlConnection 'Server=localhost\SQLEXPRESS;Database=master;Integrated Security=True;TrustServerCertificate=True'
$created = $false
try {
    $connection.Open(); $cmd = $connection.CreateCommand(); $cmd.CommandText = "CREATE DATABASE [$testDb];"; [void]$cmd.ExecuteNonQuery(); $created = $true
    foreach ($file in Get-ChildItem (Join-Path $root 'database\scripts') -Filter '*.sql' | Sort-Object Name) {
        $sql = (Get-Content $file.FullName -Raw -Encoding UTF8).Replace('aiRbPG',$testDb)
        foreach ($batch in [regex]::Split($sql,'(?im)^\s*GO\s*\r?$')) { if (![string]::IsNullOrWhiteSpace($batch)) { $cmd.CommandText = $batch; [void]$cmd.ExecuteNonQuery() } }
    }
    $env:AIRBPG_SQL_CONNECTION = "Server=localhost\SQLEXPRESS;Database=$testDb;Integrated Security=True;TrustServerCertificate=True"
    $p = Start-Process (Join-Path $bin 'aiRbPG.Interface.exe') -PassThru -Wait
    if ($p.ExitCode -ne 0) { throw 'Aplicativo de QA retornou falha.' }
}
finally {
    if ($created -and $testDb -match '^aiRbPG_UI_[0-9a-f]{32}$') { $cmd.CommandText = "USE master; ALTER DATABASE [$testDb] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$testDb];"; [void]$cmd.ExecuteNonQuery(); Write-Host 'Banco de QA removido.' }
    $connection.Dispose()
}
