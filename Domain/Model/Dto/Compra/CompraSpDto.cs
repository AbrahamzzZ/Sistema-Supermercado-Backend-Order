using Microsoft.EntityFrameworkCore;

namespace Domain.Model.Dto.Compra
{
    public class CompraSpDto
    {
        public int Id_Compra { get; set; }
        public int Id_Usuario { get; set; }
        public int Id_Sucursal { get; set; }
        public int Id_Proveedor { get; set; }
        public int Id_Transportista { get; set; }
        public string? Tipo_Documento { get; set; }
        public string? Numero_Documento { get; set; }
        public decimal Monto_Total { get; set; }
        public string? Fecha_Compra { get; set; }
    }
}
