USE IRouteCommerce;
GO

CREATE OR ALTER PROCEDURE dbo.sp_create_commerce
    @commerces dbo.commerce_type READONLY
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.commerce
        (pc_processdate, pc_codcomred, pc_nomcomred, pc_tipdoc, pc_numdoc,
         pc_ciudad, pc_direccion, pc_telefono, pc_email)
    SELECT pc_processdate, pc_codcomred, pc_nomcomred, pc_tipdoc, pc_numdoc,
           pc_ciudad, pc_direccion, pc_telefono, pc_email
    FROM @commerces;

    SELECT @@ROWCOUNT AS inserted;
END;
GO
