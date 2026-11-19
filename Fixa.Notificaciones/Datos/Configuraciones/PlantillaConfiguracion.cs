using Fixa.Notificaciones.Modelos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fixa.Notificaciones.Datos.Configuraciones;

public class PlantillaConfiguracion : IEntityTypeConfiguration<Plantilla>
{
    public void Configure(EntityTypeBuilder<Plantilla> plantilla)
    {
        plantilla.ToTable("Plantillas", tabla =>
            tabla.HasCheckConstraint("CK_Plantillas_AsuntoDeCorreo", "[Canal] <> 'Correo' OR [Asunto] IS NOT NULL"));

        plantilla.HasKey(p => p.Id);
        plantilla.Property(p => p.Codigo).HasMaxLength(40).IsRequired();
        plantilla.HasIndex(p => p.Codigo).IsUnique();
        plantilla.Property(p => p.Nombre).HasMaxLength(80).IsRequired();
        plantilla.Property(p => p.Canal).HasConversion<string>().HasMaxLength(10);
        plantilla.Property(p => p.Asunto).HasMaxLength(120);
        plantilla.Property(p => p.Cuerpo).HasMaxLength(500).IsRequired();

        plantilla.HasData(
            new Plantilla
            {
                Id = 1, Codigo = "caso_recibido", Nombre = "Reporte recibido", Canal = CanalAviso.Push,
                Cuerpo = "Recibimos tu reporte {codigo}. Estado: {estado}.", Activa = true,
            },
            new Plantilla
            {
                Id = 2, Codigo = "caso_programado", Nombre = "Atención programada", Canal = CanalAviso.Push,
                Cuerpo = "Tu reporte {codigo} ya tiene una cuadrilla asignada. Estado: {estado}.", Activa = true,
            },
            new Plantilla
            {
                Id = 3, Codigo = "caso_cerrado", Nombre = "Caso cerrado", Canal = CanalAviso.Correo,
                Asunto = "Tu reporte en FIXA fue atendido",
                Cuerpo = "El problema de tu reporte {codigo} fue solucionado. Estado: {estado}. Tienes 15 días para confirmar si quedaste conforme.",
                Activa = true,
            });
    }
}
