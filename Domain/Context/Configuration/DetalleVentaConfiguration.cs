using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Context.Configuration
{
    public class DetalleVentaConfiguration : IEntityTypeConfiguration<DetalleVenta>
    {
        public void Configure(EntityTypeBuilder<DetalleVenta> builder)
        {
            builder.ToTable("DETALLE_VENTA");
            builder.HasKey(e => e.IdDetalleVenta);

            builder.Property(e => e.IdDetalleVenta).HasColumnName("ID_DETALLE_VENTA");
            builder.Property(e => e.Cantidad).HasColumnName("CANTIDAD");
            builder.Property(e => e.Descuento)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("DESCUENTO");
            builder.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("FECHA_REGISTRO");
            builder.Property(e => e.IdProducto).HasColumnName("ID_PRODUCTO");
            builder.Property(e => e.IdVenta).HasColumnName("ID_VENTA");
            builder.Property(e => e.PrecioVenta)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("PRECIO_VENTA");
            builder.Property(e => e.Subtotal)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("SUBTOTAL");

            builder.HasOne(d => d.IdVentaNavigation)
                .WithMany(p => p.DetalleVenta)
                .HasForeignKey(d => d.IdVenta)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
