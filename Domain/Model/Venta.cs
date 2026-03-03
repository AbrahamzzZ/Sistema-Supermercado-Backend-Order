namespace Domain.Model;

public class Venta
{
    public int IdVenta { get; set; }

    public int? IdUsuario { get; set; }

    public int? IdSucursal { get; set; }

    public int? IdCliente { get; set; }

    public string? TipoDocumento { get; set; }

    public string? NumeroDocumento { get; set; }

    public decimal? MontoPago { get; set; }

    public decimal? MontoCambio { get; set; }

    public decimal? MontoTotal { get; set; }

    public decimal? Descuento { get; set; }

    public DateTime? FechaVenta { get; set; }

    public virtual ICollection<DetalleVenta> DetalleVenta { get; set; } = new List<DetalleVenta>();
}
