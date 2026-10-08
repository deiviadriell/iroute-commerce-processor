USE IRouteCommerce;
GO

CREATE OR ALTER PROCEDURE dbo.sp_process_commerce
    @processdate DATE
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    WITH evaluated AS
    (
        SELECT c.*,
               CONCAT_WS(N'; ',
                   CASE WHEN NULLIF(LTRIM(RTRIM(c.pc_nomcomred)), N'') IS NULL
                        THEN N'El nombre del comercio (nomcomred) se encuentra vacío' END,
                   CASE WHEN NULLIF(LTRIM(RTRIM(c.pc_numdoc)), '') IS NULL
                        THEN N'El número de documento (numdoc) se encuentra vacío' END,
                   CASE WHEN c.pc_numdoc LIKE '%[A-Za-z]%' COLLATE Latin1_General_CI_AI
                        THEN N'El número de documento (numdoc) contiene letras' END,
                   CASE WHEN NULLIF(LTRIM(RTRIM(c.pc_numdoc)), '') IS NOT NULL
                         AND c.pc_numdoc LIKE '%[^0-9A-Za-z]%' COLLATE Latin1_General_CI_AI
                        THEN N'El número de documento (numdoc) contiene caracteres especiales' END
               ) AS motivo
        FROM dbo.commerce AS c
        WHERE c.pc_processdate = @processdate
    )
    DELETE FROM evaluated
    OUTPUT deleted.id, deleted.pc_processdate, deleted.pc_codcomred, deleted.pc_nomcomred,
           deleted.pc_tipdoc, deleted.pc_numdoc, deleted.pc_ciudad, deleted.pc_direccion,
           deleted.pc_telefono, deleted.pc_email, deleted.motivo
    INTO dbo.commerce_quarantine
        (commerce_id, pc_processdate, pc_codcomred, pc_nomcomred, pc_tipdoc, pc_numdoc,
         pc_ciudad, pc_direccion, pc_telefono, pc_email, motivo)
    WHERE evaluated.motivo <> N'';

    DECLARE @quarantined INT = @@ROWCOUNT;

    COMMIT TRANSACTION;

    SELECT @quarantined AS quarantined;
END;
GO
