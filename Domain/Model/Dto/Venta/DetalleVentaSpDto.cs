namespace Domain.Model.Dto.Venta
{
    public class DetalleVentaSpDto
    {
        public int Id_Producto { get; set; }
        public decimal Precio_Venta { get; set; }
        public int Cantidad { get; set; }
        public decimal SubTotal { get; set; }
    }
}
