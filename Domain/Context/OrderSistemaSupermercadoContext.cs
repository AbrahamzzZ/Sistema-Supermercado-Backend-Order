using Domain.Model;
using Domain.Model.Dto.Compra;
using Domain.Model.Dto.Venta;
using Microsoft.EntityFrameworkCore;

namespace Domain.Context
{
    public partial class OrderSistemaSupermercadoContext : DbContext
    {
        public virtual DbSet<Compra> Compras { get; set; }

        public virtual DbSet<DetalleCompra> DetalleCompras { get; set; }

        public virtual DbSet<DetalleVenta> DetalleVenta { get; set; }

        public virtual DbSet<Log> Logs { get; set; }

        public virtual DbSet<Venta> Venta { get; set; }

        public DbSet<CompraRespuesta> CompraDto { get; set; }

        public DbSet<DetalleCompras> DetalleComprasDto { get; set; }

        public DbSet<DetalleComprasRepuesta> DetalleComprasRepuestaDto { get; set; }

        public DbSet<VentaRespuesta> VentaDto { get; set; }

        public DbSet<DetalleVentas> DetalleVentasDto { get; set; }

        public DbSet<DetalleVentasRepuesta> DetalleVentasRepuestaDto { get; set; }

        public OrderSistemaSupermercadoContext(DbContextOptions<OrderSistemaSupermercadoContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Clases serializadas
            modelBuilder.Entity<CompraRespuesta>().HasNoKey()
            .ToView(null)
            .Property(c => c.Monto_Total)
            .HasPrecision(18, 2);

            modelBuilder.Entity<CompraSpDto>().HasNoKey().ToView(null);

            modelBuilder.Entity<DetalleCompraSpDto>().HasNoKey().ToView(null);

            modelBuilder.Entity<VentaSpDto>().HasNoKey().ToView(null);

            modelBuilder.Entity<DetalleCompras>(entity =>
            {
                entity.HasNoKey()
                      .ToView(null);

                entity.Property(e => e.Precio_Compra)
                      .HasPrecision(18, 2);

                entity.Property(e => e.Precio_Venta)
                      .HasPrecision(18, 2);

                entity.Property(e => e.SubTotal)
                      .HasPrecision(18, 2);
            });

            modelBuilder.Entity<DetalleComprasRepuesta>(entity =>
            {
                entity.HasNoKey()
                      .ToView(null);

                entity.Property(e => e.Precio_Compra)
                       .HasPrecision(18, 2);

                entity.Property(e => e.Precio_Venta)
                      .HasPrecision(18, 2);

                entity.Property(e => e.SubTotal)
                      .HasPrecision(18, 2);
            });

            modelBuilder.Entity<VentaRespuesta>(entity =>
            {
                entity.HasNoKey()
                      .ToView(null);

                entity.Property(e => e.Monto_Cambio)
                      .HasPrecision(18, 2);

                entity.Property(e => e.Monto_Pago)
                      .HasPrecision(18, 2);

                entity.Property(e => e.Monto_Total)
                      .HasPrecision(18, 2);

                entity.Property(c => c.Descuento)
                      .HasPrecision(18, 2);
            });

            modelBuilder.Entity<DetalleVentas>(entity =>
            {
                entity.HasNoKey()
                .ToView(null);

                entity.Property(c => c.Descuento)
                      .HasPrecision(18, 2);

                entity.Property(e => e.Precio_Venta)
                      .HasPrecision(18, 2);

                entity.Property(e => e.SubTotal)
                      .HasPrecision(18, 2);

            });

            modelBuilder.Entity<DetalleVentasRepuesta>(entity =>
            {
                entity.HasNoKey()
                      .ToView(null);

                entity.Property(e => e.Precio_Venta)
                      .HasPrecision(18, 2);

                entity.Property(e => e.SubTotal)
                      .HasPrecision(18, 2);

                entity.Property(e => e.Descuento)
                      .HasPrecision(18, 2);
            });

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderSistemaSupermercadoContext).Assembly);
        }
    }
}
