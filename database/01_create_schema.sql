-- Cria o banco e a tabela usados pela aplicação. Pode ser executado mais de uma vez (idempotente).
IF DB_ID(N'PortalDeCurriculosDb') IS NULL
    CREATE DATABASE PortalDeCurriculosDb;
GO

USE PortalDeCurriculosDb;
GO

IF OBJECT_ID(N'dbo.Candidatos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Candidatos (
        Id                  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Candidatos PRIMARY KEY,
        NomeCompleto        NVARCHAR(150)  NOT NULL,
        Email               NVARCHAR(254)  NOT NULL,
        Telefone            NVARCHAR(30)   NULL,
        AreaInteresse       NVARCHAR(100)  NULL,
        ResumoProfissional  NVARCHAR(4000) NULL,
        CriadoEm            DATETIME2      NOT NULL CONSTRAINT DF_Candidatos_CriadoEm DEFAULT SYSUTCDATETIME()
    );

    CREATE INDEX IX_Candidatos_NomeCompleto ON dbo.Candidatos (NomeCompleto);
    CREATE INDEX IX_Candidatos_Email ON dbo.Candidatos (Email);
END
GO
