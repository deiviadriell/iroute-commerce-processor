USE IRouteCommerce;
GO

IF OBJECT_ID(N'dbo.users', N'U') IS NULL
    CREATE TABLE dbo.users
    (
        id            INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_users PRIMARY KEY,
        username      VARCHAR(50)   NOT NULL CONSTRAINT UQ_users_username UNIQUE,
        password_hash VARCHAR(200)  NOT NULL,
        full_name     NVARCHAR(150) NOT NULL,
        is_active     BIT           NOT NULL CONSTRAINT DF_users_is_active DEFAULT 1,
        created_at    DATETIME2(0)  NOT NULL CONSTRAINT DF_users_created_at DEFAULT SYSDATETIME()
    );
GO

CREATE OR ALTER PROCEDURE dbo.sp_get_user_by_username
    @username VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT id, username, password_hash, full_name, is_active
    FROM dbo.users
    WHERE username = @username;
END;
GO
