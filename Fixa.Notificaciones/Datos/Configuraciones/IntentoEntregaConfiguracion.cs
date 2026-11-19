using Fixa.Notificaciones.Modelos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fixa.Notificaciones.Datos.Configuraciones;

public class IntentoEntregaConfiguracion : IEntityTypeConfiguration<IntentoEntrega>
{
    public void Configure(EntityTypeBuilder<IntentoEntrega> intento)
    {
        intento.ToTable("IntentosEntrega");
        intento.HasKey(i => i.Id);
        intento.Property(i => i.Fecha).HasDefaultValueSql("SYSUTCDATETIME()");
        intento.Property(i => i.Detalle).HasMaxLength(250).IsRequired();
    }
}
