using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Context.Configuration
{
    public class LogConfiguration : IEntityTypeConfiguration<Log>
    {
        public void Configure(EntityTypeBuilder<Log> builder)
        {
            builder.ToTable("LOG");
            builder.HasKey(e => e.Id_Log);

            builder.Property(e => e.Id_Log).HasColumnName("ID_LOG");
            builder.Property(e => e.Codigo_Error)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("CODIGO_ERROR");
            builder.Property(e => e.Detalle_Error)
                .IsUnicode(false)
                .HasColumnName("DETALLE_ERROR");
            builder.Property(e => e.Endpoint)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("ENDPOINT");
            builder.Property(e => e.Fecha)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("FECHA");
            builder.Property(e => e.Id_Usuario).HasColumnName("ID_USUARIO");
            builder.Property(e => e.Mensaje_Error)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("MENSAJE_ERROR");
            builder.Property(e => e.Metodo)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("METODO");
            builder.Property(e => e.Nivel)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("ERROR")
                .HasColumnName("NIVEL");
        }
    }
}
