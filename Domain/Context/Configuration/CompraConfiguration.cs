using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Context.Configuration
{
    public class CompraConfiguration : IEntityTypeConfiguration<Compra>
    {
        public void Configure(EntityTypeBuilder<Compra> builder)
        {
            builder.ToTable("COMPRA");
            builder.HasKey(e => e.IdCompra);

            builder.HasIndex(e => e.NumeroDocumento, "UQ__COMPRA__87B6EC7EAF707742").IsUnique();

            builder.Property(e => e.IdCompra).HasColumnName("ID_COMPRA");
            builder.Property(e => e.FechaCompra)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("FECHA_COMPRA");
            builder.Property(e => e.IdProveedor).HasColumnName("ID_PROVEEDOR");
            builder.Property(e => e.IdSucursal).HasColumnName("ID_SUCURSAL");
            builder.Property(e => e.IdTransportista).HasColumnName("ID_TRANSPORTISTA");
            builder.Property(e => e.IdUsuario).HasColumnName("ID_USUARIO");
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
