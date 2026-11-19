using Fixa.Notificaciones.Datos;
using Fixa.Notificaciones.Dtos;
using Fixa.Notificaciones.Modelos;
using Microsoft.EntityFrameworkCore;

namespace Fixa.Notificaciones.Servicios;

/// <summary>Error de regla de negocio o de referencia inválida: el controlador responde 400.</summary>
public sealed class ErrorDeDatos(string campo, string mensaje) : Exception(mensaje)
{
    public string Campo { get; } = campo;
}

/// <summary>
/// Crea los avisos y los entrega. Elige la subclase según el canal de la plantilla y delega el
/// envío en el canal que corresponde, así agregar un canal nuevo no cambia esta clase.
/// </summary>
public sealed class ServicioAvisos(NotificacionesDbContext db, IEnumerable<ICanalEnvio> canales)
{
    public async Task<Aviso> CrearYEnviarAsync(AvisoEntrada entrada, CancellationToken cancelacion)
    {
        var plantilla = await db.Plantillas.FirstOrDefaultAsync(p => p.Codigo == entrada.CodigoPlantilla.Trim(), cancelacion)
            ?? throw new ErrorDeDatos(nameof(entrada.CodigoPlantilla), "La plantilla indicada no existe.");
        if (!plantilla.Activa)
            throw new ErrorDeDatos(nameof(entrada.CodigoPlantilla), "La plantilla está desactivada.");

        var destinatario = await db.Destinatarios.FirstOrDefaultAsync(d => d.CodigoReporte == entrada.CodigoReporte, cancelacion)
            ?? throw new ErrorDeDatos(nameof(entrada.CodigoReporte), "No hay un destinatario registrado con ese código de reporte.");

        var mensaje = plantilla.Cuerpo
            .Replace("{codigo}", destinatario.CodigoReporte)
            .Replace("{estado}", entrada.EstadoCaso.Trim());

        Aviso aviso = plantilla.Canal switch
        {
            CanalAviso.Push => new AvisoPush
            {
                TokenDispositivo = destinatario.TokenDispositivo
                    ?? throw new ErrorDeDatos(nameof(entrada.CodigoReporte), "El destinatario no registró un dispositivo para avisos push."),
                Titulo = plantilla.Nombre,
            },
            _ => new AvisoCorreo
            {
                Correo = destinatario.Correo
                    ?? throw new ErrorDeDatos(nameof(entrada.CodigoReporte), "El destinatario no registró un correo."),
                Asunto = plantilla.Asunto!,
            },
        };
        aviso.PlantillaId = plantilla.Id;
        aviso.DestinatarioId = destinatario.Id;
        aviso.Mensaje = mensaje;
        aviso.FechaCreacion = DateTime.UtcNow;

        db.Avisos.Add(aviso);
        await EntregarAsync(aviso, cancelacion);
        return aviso;
    }

    /// <summary>
    /// Vuelve a intentar un aviso que falló, con el contacto actual del destinatario: si el
    /// vecino corrigió su correo o su dispositivo, el reintento usa el dato nuevo.
    /// </summary>
    public async Task<Aviso?> ReintentarAsync(int id, CancellationToken cancelacion)
    {
        var aviso = await db.Avisos.Include(a => a.Destinatario).FirstOrDefaultAsync(a => a.Id == id, cancelacion);
        if (aviso is null)
            return null;
        if (aviso.Estado != EstadoAviso.Fallido)
            throw new ErrorDeDatos("id", "Solo se pueden reintentar avisos fallidos.");

        switch (aviso)
        {
            case AvisoPush push when aviso.Destinatario.TokenDispositivo is { } token:
                push.TokenDispositivo = token;
                break;
            case AvisoCorreo correo when aviso.Destinatario.Correo is { } direccion:
                correo.Correo = direccion;
                break;
        }

        await EntregarAsync(aviso, cancelacion);
        return aviso;
    }

    private async Task EntregarAsync(Aviso aviso, CancellationToken cancelacion)
    {
        var resultado = await canales.Single(c => c.Canal == aviso.Canal).EnviarAsync(aviso, cancelacion);
        var ahora = DateTime.UtcNow;

        aviso.Intentos.Add(new IntentoEntrega { Fecha = ahora, Exitoso = resultado.Exitoso, Detalle = resultado.Detalle });
        aviso.Estado = resultado.Exitoso ? EstadoAviso.Enviado : EstadoAviso.Fallido;
        aviso.FechaEnvio = resultado.Exitoso ? ahora : null;

        await db.SaveChangesAsync(cancelacion);
    }
}
