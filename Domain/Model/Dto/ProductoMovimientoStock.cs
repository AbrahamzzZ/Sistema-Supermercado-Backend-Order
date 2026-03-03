namespace Domain.Model.Dto
{
    public class ProductoMovimientoStock
    {
        public int IdProducto { get; set; }
        public string? Tipo { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
        public string? Referencia { get; set; }
    }
}
