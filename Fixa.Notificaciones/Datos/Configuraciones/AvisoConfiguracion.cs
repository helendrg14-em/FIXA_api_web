using Fixa.Notificaciones.Modelos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fixa.Notificaciones.Datos.Configuraciones;

public class AvisoConfiguracion : IEntityTypeConfiguration<Aviso>
{
    public void Configure(EntityTypeBuilder<Aviso> aviso)
    {
        aviso.ToTable("Avisos");
        aviso.HasKey(a => a.Id);

        // Generalización en una sola tabla: la columna "Canal" indica qué subclase es cada fila.
        // La propiedad Aviso.Canal se calcula en C#; el discriminador es una propiedad aparte.
        aviso.Ignore(a => a.Canal);
        aviso.HasDiscriminator<string>("Discriminador")
            .HasValue<AvisoPush>(nameof(CanalAviso.Push))
            .HasValue<AvisoCorreo>(nameof(CanalAviso.Correo));
        aviso.Property<string>("Discriminador").HasColumnName("Canal").HasMaxLength(10);

        aviso.Property(a => a.Mensaje).HasMaxLength(500).IsRequired();
        aviso.Property(a => a.Estado).HasConversion<string>().HasMaxLength(10);
        aviso.Property(a => a.FechaCreacion).HasDefaultValueSql("SYSUTCDATETIME()");
        aviso.HasIndex(a => a.Estado);

        // Restrict: una plantilla o un destinatario con avisos no se pueden borrar (queda el historial).
        aviso.HasOne(a => a.Plantilla).WithMany(p => p.Avisos)
            .HasForeignKey(a => a.PlantillaId).OnDelete(DeleteBehavior.Restrict);
        aviso.HasOne(a => a.Destinatario).WithMany(d => d.Avisos)
            .HasForeignKey(a => a.DestinatarioId).OnDelete(DeleteBehavior.Restrict);

        // Composición: si se borra el aviso, se borran sus intentos.
        aviso.HasMany(a => a.Intentos).WithOne(i => i.Aviso)
            .HasForeignKey(i => i.AvisoId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class AvisoPushConfiguracion : IEntityTypeConfiguration<AvisoPush>
{
    public void Configure(EntityTypeBuilder<AvisoPush> push)
    {
        push.Property(p => p.TokenDispositivo).HasMaxLength(255).IsRequired();
        push.Property(p => p.Titulo).HasMaxLength(80).IsRequired();
    }
}

public class AvisoCorreoConfiguracion : IEntityTypeConfiguration<AvisoCorreo>
{
    public void Configure(EntityTypeBuilder<AvisoCorreo> correo)
    {
        correo.Property(c => c.Correo).HasMaxLength(120).IsRequired();
        correo.Property(c => c.Asunto).HasMaxLength(120).IsRequired();
    }
}
