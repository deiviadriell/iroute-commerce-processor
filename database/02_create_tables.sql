USE IRouteCommerce;
GO

IF OBJECT_ID(N'dbo.commerce', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.commerce
    (
        id             BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_commerce PRIMARY KEY,
        pc_processdate DATE          NOT NULL,
        pc_codcomred   VARCHAR(30)   NULL,
        pc_nomcomred   NVARCHAR(150) NULL,
        pc_tipdoc      VARCHAR(10)   NULL,
        pc_numdoc      VARCHAR(30)   NULL,
        pc_ciudad      NVARCHAR(80)  NULL,
        pc_direccion   NVARCHAR(250) NULL,
        pc_telefono    VARCHAR(30)   NULL,
        pc_email       VARCHAR(120)  NULL,
        created_at     DATETIME2(0)  NOT NULL CONSTRAINT DF_commerce_created_at DEFAULT SYSDATETIME()
    );

    CREATE INDEX IX_commerce_processdate ON dbo.commerce (pc_processdate);
END;
GO

IF OBJECT_ID(N'dbo.commerce_quarantine', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.commerce_quarantine
    (
        id             BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_commerce_quarantine PRIMARY KEY,
        commerce_id    BIGINT        NOT NULL,
        pc_processdate DATE          NOT NULL,
        pc_codcomred   VARCHAR(30)   NULL,
        pc_nomcomred   NVARCHAR(150) NULL,
        pc_tipdoc      VARCHAR(10)   NULL,
        pc_numdoc      VARCHAR(30)   NULL,
        pc_ciudad      NVARCHAR(80)  NULL,
        pc_direccion   NVARCHAR(250) NULL,
        pc_telefono    VARCHAR(30)   NULL,
        pc_email       VARCHAR(120)  NULL,
        quarantined_at DATETIME2(0)  NOT NULL CONSTRAINT DF_commerce_quarantine_quarantined_at DEFAULT SYSDATETIME()
    );

    CREATE INDEX IX_commerce_quarantine_processdate ON dbo.commerce_quarantine (pc_processdate);
END;
GO

IF TYPE_ID(N'dbo.commerce_type') IS NULL
    CREATE TYPE dbo.commerce_type AS TABLE
    (
        pc_processdate DATE          NOT NULL,
        pc_codcomred   VARCHAR(30)   NULL,
        pc_nomcomred   NVARCHAR(150) NULL,
        pc_tipdoc      VARCHAR(10)   NULL,
        pc_numdoc      VARCHAR(30)   NULL,
        pc_ciudad      NVARCHAR(80)  NULL,
        pc_direccion   NVARCHAR(250) NULL,
        pc_telefono    VARCHAR(30)   NULL,
        pc_email       VARCHAR(120)  NULL
    );
GO
