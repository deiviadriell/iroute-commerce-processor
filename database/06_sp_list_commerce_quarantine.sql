USE IRouteCommerce;
GO

CREATE OR ALTER PROCEDURE dbo.sp_list_commerce_quarantine
    @processdate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT id, commerce_id, pc_processdate, pc_codcomred, pc_nomcomred, pc_tipdoc, pc_numdoc,
           pc_ciudad, pc_direccion, pc_telefono, pc_email, motivo, quarantined_at
    FROM dbo.commerce_quarantine
    WHERE @processdate IS NULL OR pc_processdate = @processdate
    ORDER BY pc_processdate DESC, id;
END;
GO
