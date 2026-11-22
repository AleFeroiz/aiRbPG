# Banco SQL Server

O banco será executado em uma instância de **Microsoft SQL Server hospedada em um computador Windows**. A instância pode ser SQL Server Express local ou um servidor da Fatec; o grupo confirmou o produto e o sistema operacional do servidor, mas ainda precisa definir qual computador/instância usará na demonstração.

## Criar o banco

Abra `scripts/001_create_database.sql` no SQL Server Management Studio ou execute-o com `sqlcmd`. O script cria o banco `aiRbPG`, índices para consultas por período e alguns dados de demonstração. O primeiro Administrador será criado pelo aplicativo na primeira execução, para que a senha seja gravada com hash e salt.

## Configuração da conexão no Windows

O aplicativo lê a variável de ambiente `AIRBPG_SQL_CONNECTION`. Para SQL Server Express com autenticação integrada do Windows, configure no PowerShell:

```powershell
[Environment]::SetEnvironmentVariable(
  "AIRBPG_SQL_CONNECTION",
  "Server=localhost\SQLEXPRESS;Database=aiRbPG;Integrated Security=True;TrustServerCertificate=True",
  "User"
)
```

Feche e reabra o Visual Studio para ele ler a variável. Se usar outra instância, ajuste o `Server`. Não adicione senhas ou arquivos de conexão locais ao Git.

## Observação sobre preço

O grupo ainda precisa aprovar a fórmula. A estimativa implementada considera tarifas por hora para espaço e acessório e uma tarifa opcional por pessoa/hora; a reserva guarda os valores aplicados para preservar o histórico.

## Preparação automática local

Com SQL Server Express instalado e o usuário Windows autorizado a criar bancos, execute da raiz:

```powershell
powershell -NoProfile -File scripts\configurar-banco.ps1
```

Para outra instância, acrescente `-Servidor "SERVIDOR\INSTANCIA"`. O script aplica o SQL inicial em lotes GO e salva a conexão integrada para o usuário atual. Ele não instala SQL Server nem cria uma conta de administrador do aplicativo. Reabra a IDE após executá-lo. As evidências do ambiente preparado estão em [docs/PROGRESSO.md](../docs/PROGRESSO.md).
