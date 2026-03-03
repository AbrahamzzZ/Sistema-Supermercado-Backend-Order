using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Context.Configuration
{
    public class VentaConfiguration : IEntityTypeConfiguration<Venta>
    {
        public void Configure(EntityTypeBuilder<Venta> builder)
        {
            builder.ToTable("VENTA");
            builder.HasKey(e => e.IdVenta);

            builder.HasIndex(e => e.NumeroDocumento, "UQ__VENTA__87B6EC7E76F1FEDA").IsUnique();

            builder.Property(e => e.IdVenta).HasColumnName("ID_VENTA");
            builder.Property(e => e.Descuento)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("DESCUENTO");
            builder.Property(e => e.FechaVenta)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("FECHA_VENTA");
            builder.Property(e => e.IdCliente).HasColumnName("ID_CLIENTE");
            builder.Property(e => e.IdSucursal).HasColumnName("ID_SUCURSAL");
            builder.Property(e => e.IdUsuario).HasColumnName("ID_USUARIO");
            builder.Property(e => e.MontoCambio)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("MONTO_CAMBIO");
            builder.Property(e => e.MontoPago)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("MONTO_PAGO");
            builder.Property(e => e.MontoTotal)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("MONTO_TOTAL");
            builder.Property(e => e.NumeroDocumento)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("NUMERO_DOCUMENTO");
            builder.Property(e => e.TipoDocumento)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("TIPO_DOCUMENTO");

        }
    }
}
