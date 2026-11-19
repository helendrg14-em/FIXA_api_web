using Fixa.Notificaciones.Modelos;
using Microsoft.EntityFrameworkCore;

namespace Fixa.Notificaciones.Datos;

public class NotificacionesDbContext(DbContextOptions<NotificacionesDbContext> opciones) : DbContext(opciones)
{
    public DbSet<Plantilla> Plantillas => Set<Plantilla>();

    public DbSet<Destinatario> Destinatarios => Set<Destinatario>();

    public DbSet<Aviso> Avisos => Set<Aviso>();

    public DbSet<IntentoEntrega> IntentosEntrega => Set<IntentoEntrega>();

    // Cada entidad tiene su clase en Datos/Configuraciones (claves, relaciones y datos de prueba).
    protected override void OnModelCreating(ModelBuilder modelo) =>
        modelo.ApplyConfigurationsFromAssembly(typeof(NotificacionesDbContext).Assembly);
}
