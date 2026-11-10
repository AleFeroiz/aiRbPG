IF DB_ID(N'aiRbPG') IS NULL
BEGIN
    CREATE DATABASE aiRbPG;
END;
GO

USE aiRbPG;
GO

IF OBJECT_ID(N'dbo.Usuarios', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Usuarios
    (
        UsuarioId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Usuarios PRIMARY KEY,
        Nome NVARCHAR(120) NOT NULL,
        Login NVARCHAR(60) NOT NULL,
        Perfil NVARCHAR(20) NOT NULL,
        SenhaSalt VARBINARY(16) NOT NULL,
        SenhaHash VARBINARY(32) NOT NULL,
        IteracoesHash INT NOT NULL,
        Ativo BIT NOT NULL CONSTRAINT DF_Usuarios_Ativo DEFAULT (1),
        CriadoEm DATETIME2(0) NOT NULL CONSTRAINT DF_Usuarios_CriadoEm DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_Usuarios_Login UNIQUE (Login),
        CONSTRAINT CK_Usuarios_Perfil CHECK (Perfil IN (N'Administrador', N'Atendente')),
        CONSTRAINT CK_Usuarios_Iteracoes CHECK (IteracoesHash >= 100000)
    );
END;
GO

IF OBJECT_ID(N'dbo.Clientes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Clientes
    (
        ClienteId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Clientes PRIMARY KEY,
        Nome NVARCHAR(120) NOT NULL,
        Telefone NVARCHAR(30) NULL,
        Email NVARCHAR(254) NULL,
        Ativo BIT NOT NULL CONSTRAINT DF_Clientes_Ativo DEFAULT (1),
        CriadoEm DATETIME2(0) NOT NULL CONSTRAINT DF_Clientes_CriadoEm DEFAULT (SYSUTCDATETIME())
    );
END;
GO

IF OBJECT_ID(N'dbo.Espacos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Espacos
    (
        EspacoId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Espacos PRIMARY KEY,
        Nome NVARCHAR(100) NOT NULL,
        Descricao NVARCHAR(500) NULL,
        Endereco NVARCHAR(250) NULL,
        Capacidade INT NOT NULL,
        TarifaHora DECIMAL(10,2) NOT NULL,
        TarifaPessoaHora DECIMAL(10,2) NOT NULL CONSTRAINT DF_Espacos_TarifaPessoaHora DEFAULT (0),
        Ativo BIT NOT NULL CONSTRAINT DF_Espacos_Ativo DEFAULT (1),
        CONSTRAINT CK_Espacos_Capacidade CHECK (Capacidade > 0),
        CONSTRAINT CK_Espacos_TarifaHora CHECK (TarifaHora >= 0),
        CONSTRAINT CK_Espacos_TarifaPessoaHora CHECK (TarifaPessoaHora >= 0)
    );
END;
GO

IF OBJECT_ID(N'dbo.SistemasRpg', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SistemasRpg
    (
        SistemaRpgId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SistemasRpg PRIMARY KEY,
        Nome NVARCHAR(100) NOT NULL,
        Descricao NVARCHAR(500) NULL,
        Ativo BIT NOT NULL CONSTRAINT DF_SistemasRpg_Ativo DEFAULT (1),
        CONSTRAINT UQ_SistemasRpg_Nome UNIQUE (Nome)
    );
END;
GO

IF OBJECT_ID(N'dbo.Acessorios', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Acessorios
    (
        AcessorioId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Acessorios PRIMARY KEY,
        Nome NVARCHAR(120) NOT NULL,
        Descricao NVARCHAR(500) NULL,
        QuantidadeEstoque INT NOT NULL,
        TarifaHora DECIMAL(10,2) NOT NULL,
        Ativo BIT NOT NULL CONSTRAINT DF_Acessorios_Ativo DEFAULT (1),
        CONSTRAINT CK_Acessorios_Estoque CHECK (QuantidadeEstoque >= 0),
        CONSTRAINT CK_Acessorios_TarifaHora CHECK (TarifaHora >= 0)
    );
END;
GO

IF OBJECT_ID(N'dbo.SistemasAcessorios', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SistemasAcessorios
    (
        SistemaRpgId INT NOT NULL,
        AcessorioId INT NOT NULL,
        CONSTRAINT PK_SistemasAcessorios PRIMARY KEY (SistemaRpgId, AcessorioId),
        CONSTRAINT FK_SistemasAcessorios_Sistema FOREIGN KEY (SistemaRpgId) REFERENCES dbo.SistemasRpg(SistemaRpgId),
        CONSTRAINT FK_SistemasAcessorios_Acessorio FOREIGN KEY (AcessorioId) REFERENCES dbo.Acessorios(AcessorioId)
    );
END;
GO

IF OBJECT_ID(N'dbo.Kits', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Kits
    (
        KitId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Kits PRIMARY KEY,
        Nome NVARCHAR(120) NOT NULL,
        Descricao NVARCHAR(500) NULL,
        Ativo BIT NOT NULL CONSTRAINT DF_Kits_Ativo DEFAULT (1)
    );
END;
GO

IF OBJECT_ID(N'dbo.ItensKit', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ItensKit
    (
        KitId INT NOT NULL,
        AcessorioId INT NOT NULL,
        Quantidade INT NOT NULL,
        CONSTRAINT PK_ItensKit PRIMARY KEY (KitId, AcessorioId),
        CONSTRAINT FK_ItensKit_Kit FOREIGN KEY (KitId) REFERENCES dbo.Kits(KitId),
        CONSTRAINT FK_ItensKit_Acessorio FOREIGN KEY (AcessorioId) REFERENCES dbo.Acessorios(AcessorioId),
        CONSTRAINT CK_ItensKit_Quantidade CHECK (Quantidade > 0)
    );
END;
GO

IF OBJECT_ID(N'dbo.Reservas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Reservas
    (
        ReservaId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Reservas PRIMARY KEY,
        ClienteId INT NOT NULL,
        EspacoId INT NOT NULL,
        CriadaPorUsuarioId INT NOT NULL,
        Inicio DATETIME2(0) NOT NULL,
        Fim DATETIME2(0) NOT NULL,
        QuantidadePessoas INT NOT NULL,
        Status NVARCHAR(24) NOT NULL CONSTRAINT DF_Reservas_Status DEFAULT (N'Pendente'),
        Horas DECIMAL(8,2) NOT NULL,
        TarifaEspacoHoraAplicada DECIMAL(10,2) NOT NULL,
        TarifaPessoaHoraAplicada DECIMAL(10,2) NOT NULL,
        SubtotalEspaco DECIMAL(10,2) NOT NULL,
        SubtotalPessoas DECIMAL(10,2) NOT NULL,
        Desconto DECIMAL(10,2) NOT NULL CONSTRAINT DF_Reservas_Desconto DEFAULT (0),
        Multas DECIMAL(10,2) NOT NULL CONSTRAINT DF_Reservas_Multas DEFAULT (0),
        ValorTotal DECIMAL(10,2) NOT NULL,
        Observacao NVARCHAR(500) NULL,
        CriadaEm DATETIME2(0) NOT NULL CONSTRAINT DF_Reservas_CriadaEm DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_Reservas_Cliente FOREIGN KEY (ClienteId) REFERENCES dbo.Clientes(ClienteId),
        CONSTRAINT FK_Reservas_Espaco FOREIGN KEY (EspacoId) REFERENCES dbo.Espacos(EspacoId),
        CONSTRAINT FK_Reservas_Usuario FOREIGN KEY (CriadaPorUsuarioId) REFERENCES dbo.Usuarios(UsuarioId),
        CONSTRAINT CK_Reservas_Periodo CHECK (Fim > Inicio),
        CONSTRAINT CK_Reservas_Pessoas CHECK (QuantidadePessoas > 0),
        CONSTRAINT CK_Reservas_Status CHECK (Status IN (N'Pendente', N'Confirmada', N'Cancelada', N'EmAndamento', N'AguardandoDevolucao', N'Finalizada')),
        CONSTRAINT CK_Reservas_Valores CHECK (Horas > 0 AND TarifaEspacoHoraAplicada >= 0 AND TarifaPessoaHoraAplicada >= 0 AND Desconto >= 0 AND Multas >= 0 AND ValorTotal >= 0)
    );
END;
GO

IF OBJECT_ID(N'dbo.ItensReserva', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ItensReserva
    (
        ItemReservaId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ItensReserva PRIMARY KEY,
        ReservaId INT NOT NULL,
        AcessorioId INT NOT NULL,
        QuantidadeReservada INT NOT NULL,
        QuantidadeRetirada INT NOT NULL CONSTRAINT DF_ItensReserva_Retirada DEFAULT (0),
        QuantidadeDevolvida INT NOT NULL CONSTRAINT DF_ItensReserva_Devolvida DEFAULT (0),
        QuantidadeDanificada INT NOT NULL CONSTRAINT DF_ItensReserva_Danificada DEFAULT (0),
        QuantidadePerdida INT NOT NULL CONSTRAINT DF_ItensReserva_Perdida DEFAULT (0),
        TarifaHoraAplicada DECIMAL(10,2) NOT NULL,
        Subtotal DECIMAL(10,2) NOT NULL,
        CONSTRAINT FK_ItensReserva_Reserva FOREIGN KEY (ReservaId) REFERENCES dbo.Reservas(ReservaId),
        CONSTRAINT FK_ItensReserva_Acessorio FOREIGN KEY (AcessorioId) REFERENCES dbo.Acessorios(AcessorioId),
        CONSTRAINT UQ_ItensReserva_ReservaAcessorio UNIQUE (ReservaId, AcessorioId),
        CONSTRAINT CK_ItensReserva_Quantidades CHECK (QuantidadeReservada > 0 AND QuantidadeRetirada >= 0 AND QuantidadeDevolvida >= 0 AND QuantidadeDanificada >= 0 AND QuantidadePerdida >= 0),
        CONSTRAINT CK_ItensReserva_Valores CHECK (TarifaHoraAplicada >= 0 AND Subtotal >= 0)
    );
END;
GO

IF OBJECT_ID(N'dbo.Pagamentos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Pagamentos
    (
        PagamentoId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Pagamentos PRIMARY KEY,
        ReservaId INT NOT NULL,
        RegistradoPorUsuarioId INT NOT NULL,
        DataHora DATETIME2(0) NOT NULL CONSTRAINT DF_Pagamentos_DataHora DEFAULT (SYSUTCDATETIME()),
        Valor DECIMAL(10,2) NOT NULL,
        Metodo NVARCHAR(30) NOT NULL,
        Observacao NVARCHAR(250) NULL,
        CONSTRAINT FK_Pagamentos_Reserva FOREIGN KEY (ReservaId) REFERENCES dbo.Reservas(ReservaId),
        CONSTRAINT FK_Pagamentos_Usuario FOREIGN KEY (RegistradoPorUsuarioId) REFERENCES dbo.Usuarios(UsuarioId),
        CONSTRAINT CK_Pagamentos_Valor CHECK (Valor > 0)
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Reservas_Espaco_Periodo' AND object_id = OBJECT_ID(N'dbo.Reservas'))
    CREATE INDEX IX_Reservas_Espaco_Periodo ON dbo.Reservas (EspacoId, Inicio, Fim) INCLUDE (Status);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ItensReserva_Acessorio' AND object_id = OBJECT_ID(N'dbo.ItensReserva'))
    CREATE INDEX IX_ItensReserva_Acessorio ON dbo.ItensReserva (AcessorioId, ReservaId) INCLUDE (QuantidadeReservada);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.SistemasRpg WHERE Nome = N'Dungeons & Dragons')
    INSERT dbo.SistemasRpg (Nome, Descricao) VALUES (N'Dungeons & Dragons', N'Sistema de fantasia medieval para demonstração.');
IF NOT EXISTS (SELECT 1 FROM dbo.SistemasRpg WHERE Nome = N'Tormenta 20')
    INSERT dbo.SistemasRpg (Nome, Descricao) VALUES (N'Tormenta 20', N'Sistema brasileiro de fantasia para demonstração.');

IF NOT EXISTS (SELECT 1 FROM dbo.Clientes WHERE Email = N'cliente.demo@airbpg.local')
    INSERT dbo.Clientes (Nome, Telefone, Email) VALUES (N'Cliente de Demonstração', N'(15) 00000-0000', N'cliente.demo@airbpg.local');

IF NOT EXISTS (SELECT 1 FROM dbo.Espacos WHERE Nome = N'Sala da Taverna')
    INSERT dbo.Espacos (Nome, Descricao, Endereco, Capacidade, TarifaHora, TarifaPessoaHora)
    VALUES (N'Sala da Taverna', N'Sala temática com mesa para campanhas de fantasia.', N'Endereço de demonstração', 6, 80.00, 0.00);

IF NOT EXISTS (SELECT 1 FROM dbo.Acessorios WHERE Nome = N'Kit de dados poliédricos')
    INSERT dbo.Acessorios (Nome, Descricao, QuantidadeEstoque, TarifaHora)
    VALUES (N'Kit de dados poliédricos', N'Conjunto de dados para RPG de mesa.', 8, 2.00);

IF NOT EXISTS (SELECT 1 FROM dbo.Acessorios WHERE Nome = N'Miniatura de aventureiro')
    INSERT dbo.Acessorios (Nome, Descricao, QuantidadeEstoque, TarifaHora)
    VALUES (N'Miniatura de aventureiro', N'Miniatura genérica para demonstração.', 12, 1.50);
GO

INSERT INTO dbo.SistemasAcessorios (SistemaRpgId, AcessorioId)
SELECT s.SistemaRpgId, a.AcessorioId
FROM dbo.SistemasRpg s
CROSS JOIN dbo.Acessorios a
WHERE s.Nome = N'Dungeons & Dragons'
  AND a.Nome IN (N'Kit de dados poliédricos', N'Miniatura de aventureiro')
  AND NOT EXISTS
  (
      SELECT 1 FROM dbo.SistemasAcessorios sa
      WHERE sa.SistemaRpgId = s.SistemaRpgId AND sa.AcessorioId = a.AcessorioId
  );
GO
