-- =============================================================
-- V001 - Módulo Log
-- =============================================================
CREATE OR ALTER PROCEDURE PA_LISTA_LOG_PAGINACION(
    @PageNumber INT = 1,
    @PageSize INT = 10
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TotalCount INT;
    SELECT @TotalCount = COUNT(*) FROM LOG;

    -- ISNULL: el repositorio lee estas columnas con GetString()
    SELECT ID_LOG,
           ISNULL(CODIGO_ERROR, '')  AS CODIGO_ERROR,
           ISNULL(MENSAJE_ERROR, '') AS MENSAJE_ERROR,
           ISNULL(DETALLE_ERROR, '') AS DETALLE_ERROR,
           ID_USUARIO,
           FECHA,
           ISNULL(ENDPOINT, '')      AS ENDPOINT,
           ISNULL(METODO, '')        AS METODO,
           ISNULL(NIVEL, '')         AS NIVEL
    FROM LOG
    ORDER BY ID_LOG OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT @TotalCount AS TotalCount;
END;
GO

CREATE OR ALTER PROCEDURE PA_OBTENER_LOG(
    @Id_Log INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ID_LOG, CODIGO_ERROR, MENSAJE_ERROR, DETALLE_ERROR, ID_USUARIO, FECHA, ENDPOINT, METODO, NIVEL
    FROM LOG
    WHERE ID_LOG = @Id_Log;
END;
GO

CREATE OR ALTER PROCEDURE PA_REGISTRAR_LOG(
    @Codigo_Error VARCHAR(20),
    @Mensaje_Error NVARCHAR(4000),
    @Detalle_Error NVARCHAR(MAX),
    @ID_Usuario INT,
    @EndPoint VARCHAR(200),
    @Metodo VARCHAR(10),
    @Nivel VARCHAR(20)
)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO LOG (CODIGO_ERROR, MENSAJE_ERROR, DETALLE_ERROR, ID_USUARIO, ENDPOINT, METODO, NIVEL)
    VALUES (@Codigo_Error, LEFT(@Mensaje_Error, 500), @Detalle_Error, @ID_Usuario, @EndPoint, @Metodo, @Nivel);
END;
GO
