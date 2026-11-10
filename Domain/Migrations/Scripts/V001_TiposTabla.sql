-- =============================================================
-- V001 - Tipos tabla usados para enviar los detalles de compra/venta
-- =============================================================
IF TYPE_ID(N'dbo.TI_Detalle_Compra') IS NULL
    CREATE TYPE [dbo].[TI_Detalle_Compra] AS TABLE (
        [Id_Producto]   INT            NULL,
        [Precio_Compra] DECIMAL(10,2)  NULL,
        [Precio_Venta]  DECIMAL(10,2)  NULL,
        [Cantidad]      INT            NULL,
        [SubTotal]      DECIMAL(10,2)  NULL
    );
GO

IF TYPE_ID(N'dbo.TI_Detalle_Venta') IS NULL
    CREATE TYPE [dbo].[TI_Detalle_Venta] AS TABLE (
        [Id_Producto]  INT            NULL,
        [Precio_Venta] DECIMAL(10,2)  NULL,
        [Cantidad]     INT            NULL,
        [SubTotal]     DECIMAL(10,2)  NULL,
        [Descuento]    DECIMAL(10,2)  NULL
    );
GO
