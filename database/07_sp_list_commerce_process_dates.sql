USE IRouteCommerce;
GO

CREATE OR ALTER PROCEDURE dbo.sp_list_commerce_process_dates
AS
BEGIN
    SET NOCOUNT ON;

    SELECT pc_processdate, COUNT_BIG(*) AS total
    FROM dbo.commerce
    GROUP BY pc_processdate
    ORDER BY pc_processdate DESC;
END;
GO
