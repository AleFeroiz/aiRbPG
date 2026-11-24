param([string]$Servidor, [switch]$AplicativoCompilado)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent

try {
    if ($AplicativoCompilado) {
        $exe = Join-Path $root '..\Aplicativo\aiRbPG.WinForms.exe'
    } else {
        $exe = Join-Path $root 'src\aiRbPG.WinForms\bin\Release\aiRbPG.WinForms.exe'
    }
    $exe = [IO.Path]::GetFullPath($exe)
    $running = Get-Process -Name 'aiRbPG.WinForms' -ErrorAction SilentlyContinue |
        Where-Object { $_.Path -eq $exe }
    if ($running) {
        if ($Servidor) { throw 'Feche o aiRbPG antes de escolher outra conexao.' }
        Write-Host 'O aiRbPG ja esta aberto nesta pasta.'
        exit 0
    }

    if (!$AplicativoCompilado) {
        $programFiles = ${env:ProgramFiles(x86)}
        if (!$programFiles) { $programFiles = $env:ProgramFiles }
        $msbuild = Join-Path $programFiles 'MSBuild\14.0\Bin\MSBuild.exe'
        $framework = Join-Path $programFiles 'Reference Assemblies\Microsoft\Framework\.NETFramework\v4.6.1'
        if (!(Test-Path $msbuild)) {
            throw 'MSBuild 14.0 ausente. Instale Microsoft Build Tools 2015 ou Visual Studio 2015 Update 2/3 com C#.'
        }
        if (!(Test-Path (Join-Path $framework 'RedistList\FrameworkList.xml')) -or
            !(Test-Path (Join-Path $framework 'System.Windows.Forms.dll'))) {
            throw 'Referencias do .NET Framework 4.6.1 ausentes. Instale o Developer Pack 4.6.1.'
        }

        Write-Host 'Compilando Release com MSBuild 14.0...'
        & $msbuild (Join-Path $root 'aiRbPG.sln') /t:Build /p:Configuration=Release '/p:Platform=Any CPU' /nologo /verbosity:minimal
        if ($LASTEXITCODE -ne 0) { throw "MSBuild falhou (codigo $LASTEXITCODE). Confira a primeira mensagem acima." }
    }
    if (!(Test-Path $exe)) { throw 'Executavel Release ausente.' }

    & (Join-Path $PSScriptRoot 'configurar-banco.ps1') -Servidor $Servidor
    Start-Process -FilePath $exe -WorkingDirectory $root
} catch {
    [Console]::Error.WriteLine($_.Exception.Message)
    exit 1
}
