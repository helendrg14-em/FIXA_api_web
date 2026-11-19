using Fixa.Notificaciones.Modelos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fixa.Notificaciones.Datos.Configuraciones;

public class DestinatarioConfiguracion : IEntityTypeConfiguration<Destinatario>
{
    public void Configure(EntityTypeBuilder<Destinatario> destinatario)
    {
        destinatario.ToTable("Destinatarios", tabla =>
            tabla.HasCheckConstraint("CK_Destinatarios_AlgunContacto", "[Correo] IS NOT NULL OR [TokenDispositivo] IS NOT NULL"));

        destinatario.HasKey(d => d.Id);
        destinatario.Property(d => d.CodigoReporte).HasMaxLength(20).IsRequired();
        destinatario.HasIndex(d => d.CodigoReporte).IsUnique();
        destinatario.Property(d => d.Correo).HasMaxLength(120);
        destinatario.Property(d => d.TokenDispositivo).HasMaxLength(255);
        destinatario.Property(d => d.FechaRegistro).HasDefaultValueSql("SYSUTCDATETIME()");

        var fecha = new DateTime(2026, 11, 19, 12, 0, 0, DateTimeKind.Utc);
        destinatario.HasData(
            new Destinatario
            {
                Id = 1, CodigoReporte = "FX-2026-000001", TokenDispositivo = "token-de-prueba-del-celular-0001",
                FechaRegistro = fecha,
            },
            new Destinatario
            {
                Id = 2, CodigoReporte = "FX-2026-000002", Correo = "vecino.prueba@fixa.test", FechaRegistro = fecha,
            },
            // Token vencido a propósito: su aviso push falla y sirve para probar el reintento.
            new Destinatario
            {
                Id = 3, CodigoReporte = "FX-2026-000003", TokenDispositivo = "token-vencido", FechaRegistro = fecha,
            });
    }
}
