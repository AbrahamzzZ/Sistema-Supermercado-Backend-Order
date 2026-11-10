namespace Domain.Model.Dto.Inventory
{
    public class ProductoStockDto
    {
        public int IdProducto { get; set; }
        public int Stock { get; set; }
        public decimal? PrecioCompra { get; set; }
        public decimal? PrecioVenta { get; set; }
    }
}
