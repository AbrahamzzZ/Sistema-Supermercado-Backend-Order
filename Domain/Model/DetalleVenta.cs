namespace Domain.Model;

public class DetalleVenta
{
    public int IdDetalleVenta { get; set; }

    public int IdVenta { get; set; }

    public int? IdProducto { get; set; }

    public decimal? PrecioVenta { get; set; }

    public int? Cantidad { get; set; }

    public decimal? Subtotal { get; set; }

    public decimal? Descuento { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public virtual Venta? IdVentaNavigation { get; set; }
}
