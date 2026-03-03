using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Context.Configuration
{
    public class DetalleCompraConfiguration : IEntityTypeConfiguration<DetalleCompra>
    {
        public void Configure(EntityTypeBuilder<DetalleCompra> builder)
        {
            builder.ToTable("DETALLE_COMPRA");
            builder.HasKey(e => e.IdDetalleCompra);

            builder.Property(e => e.IdDetalleCompra).HasColumnName("ID_DETALLE_COMPRA");
            builder.Property(e => e.Cantidad).HasColumnName("CANTIDAD");
            builder.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("FECHA_REGISTRO");
            builder.Property(e => e.IdCompra).HasColumnName("ID_COMPRA");
            builder.Property(e => e.IdProducto).HasColumnName("ID_PRODUCTO");
            builder.Property(e => e.PrecioCompra)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("PRECIO_COMPRA");
            builder.Property(e => e.PrecioVenta)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("PRECIO_VENTA");
            builder.Property(e => e.Subtotal)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("SUBTOTAL");

            builder.HasOne(d => d.IdCompraNavigation)
                .WithMany(p => p.DetalleCompras)
                .HasForeignKey(d => d.IdCompra)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
