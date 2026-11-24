param([string]$Servidor)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Data

function New-WindowsConnection([string]$Instance) {
    $builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
    $builder['Data Source'] = $Instance
    $builder['Initial Catalog'] = 'aiRbPG'
    $builder['Integrated Security'] = $true
    $builder['TrustServerCertificate'] = $true
    $builder['Connect Timeout'] = 5
    return $builder
}

function Get-DatabaseState($Builder) {
    $probe = New-Object System.Data.SqlClient.SqlConnectionStringBuilder $Builder.ConnectionString
    $probe['Pooling'] = $false
    $connection = New-Object System.Data.SqlClient.SqlConnection $probe.ConnectionString
    try {
        $connection.Open()
        $command = $connection.CreateCommand()
        try {
            $command.CommandText = @'
SELECT COUNT(*) FROM sys.tables
WHERE schema_id = SCHEMA_ID(N'dbo') AND name IN
    (N'Usuarios',N'Clientes',N'Espacos',N'SistemasRpg',N'Acessorios',N'SistemasAcessorios',
     N'Kits',N'ItensKit',N'Reservas',N'ItensReserva',N'Pagamentos',N'HistoricoReservas');
'@
            return $command.ExecuteScalar()
        } finally { $command.Dispose() }
    } finally { $connection.Dispose() }
}

$configuration = $null
if (!$Servidor) {
    foreach ($scope in 'Process','User','Machine') {
        $value = [Environment]::GetEnvironmentVariable('AIRBPG_SQL_CONNECTION', $scope)
        if (![string]::IsNullOrWhiteSpace($value)) { $configuration = $value; break }
    }
}

if ($configuration) {
    try {
        $builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder $configuration
        if (!$builder['Data Source'] -or ($builder['Initial Catalog'] -and $builder['Initial Catalog'] -ne 'aiRbPG') -or $builder['AttachDbFilename']) {
            throw 'Configuracao incompativel.'
        }
        $builder['Initial Catalog'] = 'aiRbPG'
    } catch {
        throw 'AIRBPG_SQL_CONNECTION invalida para o banco aiRbPG. Corrija a variavel ou informe -Servidor para usar autenticacao Windows.'
    }
} elseif ($Servidor) {
    $builder = New-WindowsConnection $Servidor
} else {
    $instances = @(
        foreach ($service in Get-Service -Name 'MSSQLSERVER','MSSQL$*' -ErrorAction SilentlyContinue) {
            if ($service.Status -ne 'Running') {
                Write-Host "Servico SQL parado: $($service.Name)."
            } elseif ($service.Name -eq 'MSSQLSERVER') { 'localhost' }
            else { 'localhost\' + $service.Name.Substring(6) }
        }
    )
    $localDb = Get-Command SqlLocalDB.exe -ErrorAction SilentlyContinue
    if ($localDb) {
        $instances += @(& $localDb.Source info | Where-Object { $_.Trim() } |
            ForEach-Object { '(localdb)\' + $_.Trim() })
        if (!$instances) { $instances = @('(localdb)\MSSQLLocalDB') }
    }
    if (!$instances) {
        throw 'Nenhuma instancia SQL local em execucao e LocalDB indisponivel. Inicie o servico existente, informe -Servidor ou solicite SQL Server Express/LocalDB a TI.'
    }

    $available = @(
        foreach ($instance in $instances) {
            $candidate = New-WindowsConnection $instance
            $candidate['Initial Catalog'] = 'master'
            $connection = New-Object System.Data.SqlClient.SqlConnection $candidate.ConnectionString
            try {
                $connection.Open()
                $command = $connection.CreateCommand()
                try {
                    $command.CommandText = "SELECT CASE WHEN DB_ID(N'aiRbPG') IS NULL THEN 0 ELSE 1 END;"
                    [pscustomobject]@{ Instance = $instance; Existing = ($command.ExecuteScalar() -eq 1) }
                } finally { $command.Dispose() }
            } catch {
                $failure = $_.Exception.GetBaseException()
                Write-Host "Falha ao consultar $instance ($($failure.GetType().Name), codigo $($failure.Number))."
            } finally { $connection.Dispose() }
        }
    )
    $withDatabase = @($available | Where-Object Existing)
    $services = @($available | Where-Object { $_.Instance -notlike '(localdb)*' })
    if ($withDatabase.Count -eq 1) { $Servidor = $withDatabase[0].Instance }
    elseif ($withDatabase.Count -eq 0 -and $services.Count -eq 1) { $Servidor = $services[0].Instance }
    elseif ($available.Count -gt 1) {
        throw "Mais de uma instancia SQL disponivel. Informe -Servidor com a instancia desejada: $($available.Instance -join ', ')."
    } elseif ($available.Count -eq 1) { $Servidor = $available[0].Instance }
    else {
        throw 'As instancias encontradas recusaram a conexao. Confira o servico e solicite acesso SQL para a conta Windows.'
    }
    $builder = New-WindowsConnection $Servidor
}

$stage = 'verificar o banco aiRbPG'
try {
    $state = 0
    try { $state = Get-DatabaseState $builder }
    catch {
        $failure = $_.Exception.GetBaseException()
        if ($failure -isnot [System.Data.SqlClient.SqlException] -or $failure.Number -ne 4060) { throw }
    }

    if ($state -ne 12) {
        $stage = 'criar ou migrar o banco aiRbPG'
        $master = New-Object System.Data.SqlClient.SqlConnectionStringBuilder $builder.ConnectionString
        $master['Initial Catalog'] = 'master'
        $master['Pooling'] = $false
        $connection = New-Object System.Data.SqlClient.SqlConnection $master.ConnectionString
        try {
            $connection.Open()
            $command = $connection.CreateCommand()
            try {
                $command.CommandTimeout = 120
                $command.CommandText = @'
DECLARE @result int;
EXEC @result = sp_getapplock @Resource=N'aiRbPG:migracao', @LockMode='Exclusive',
    @LockOwner='Session', @LockTimeout=60000;
IF @result < 0 RAISERROR('Nao foi possivel bloquear a migracao aiRbPG.',16,1);
'@
                [void]$command.ExecuteNonQuery()
                try { $state = Get-DatabaseState $builder }
                catch {
                    $failure = $_.Exception.GetBaseException()
                    if ($failure -isnot [System.Data.SqlClient.SqlException] -or $failure.Number -ne 4060) { throw }
                    $state = 0
                }
                foreach ($file in Get-ChildItem (Join-Path $PSScriptRoot '..\database\scripts') -Filter '*.sql' | Sort-Object Name) {
                    if ($state -eq 12) { break }
                    if ($file.Name -eq '001_create_database.sql' -and $state -eq 11) {
                        # Only skip the base script when Usuarios and every other base table exist.
                        $command.CommandText = "SELECT OBJECT_ID(N'aiRbPG.dbo.HistoricoReservas',N'U');"
                        if ($command.ExecuteScalar() -eq [DBNull]::Value) { continue }
                    }
                    $stage = "aplicar $($file.Name)"
                    $sql = Get-Content $file.FullName -Raw -Encoding UTF8
                    foreach ($batch in [regex]::Split($sql, '(?im)^\s*GO\s*\r?$')) {
                        if ([string]::IsNullOrWhiteSpace($batch)) { continue }
                        $command.CommandText = $batch
                        [void]$command.ExecuteNonQuery()
                    }
                }
            } finally { $command.Dispose() }
        } finally { $connection.Dispose() }
        $stage = 'validar as tabelas do banco aiRbPG'
        if ((Get-DatabaseState $builder) -ne 12) {
            throw 'As tabelas esperadas, incluindo dbo.HistoricoReservas, nao estao visiveis.'
        }
    }
} catch {
    $failure = $_.Exception.GetBaseException()
    if ($failure -isnot [System.Data.SqlClient.SqlException]) { throw }
    $hint = switch ($failure.Number) {
        18456 { 'Confira a autenticacao e o login no SQL Server.' }
        4060 { 'Solicite acesso ao banco aiRbPG para o login configurado.' }
        { $_ -in 229,262,916,15151 } { 'Solicite a TI permissao SQL para criar/migrar o banco ou peca que aplique os scripts.' }
        default { 'Confira se a instancia esta acessivel e se o login tem as permissoes SQL necessarias.' }
    }
    throw "Falha ao $stage (SQL $($failure.Number), estado $($failure.State)). $hint"
}

$env:AIRBPG_SQL_CONNECTION = $builder.ConnectionString
if ($builder['Integrated Security'] -and !$configuration) {
    [Environment]::SetEnvironmentVariable('AIRBPG_SQL_CONNECTION', $builder.ConnectionString, 'User')
}
Write-Host "Banco aiRbPG e dbo.HistoricoReservas disponiveis em $($builder['Data Source'])."
