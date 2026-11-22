USE aiRbPG;
GO
IF OBJECT_ID(N'dbo.HistoricoReservas',N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HistoricoReservas
    (
        HistoricoId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ReservaId INT NOT NULL REFERENCES dbo.Reservas(ReservaId),
        UsuarioId INT NOT NULL REFERENCES dbo.Usuarios(UsuarioId),
        DataHora DATETIME2(0) NOT NULL DEFAULT (SYSDATETIME()),
        Acao NVARCHAR(60) NOT NULL,
        Observacao NVARCHAR(500) NULL
    );
END;
GO
