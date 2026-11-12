namespace Domain.Model.Dto.Estadistica
{
    public class EstadisticaTotal
    {
        public int Id { get; set; }
        public int Total { get; set; }
    }

    public class EstadisticaTendencia
    {
        public int IdProducto { get; set; }
        public DateTime Fecha { get; set; }
        public int Cantidad { get; set; }
    }
}
