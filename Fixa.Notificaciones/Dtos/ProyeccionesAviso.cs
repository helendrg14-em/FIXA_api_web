using System.Linq.Expressions;
using Fixa.Notificaciones.Modelos;

namespace Fixa.Notificaciones.Dtos;

/// <summary>
/// Conversión de entidades a DTO de avisos. La usan Avisos, Plantillas y Destinatarios, así
/// todos devuelven los avisos con la misma forma.
/// </summary>
public static class ProyeccionesAviso
{
    /// <summary>Se traduce a SQL: solo se leen las columnas del resumen.</summary>
    public static readonly Expression<Func<Aviso, AvisoResumen>> Resumen = a => new AvisoResumen(
        a.Id,
        a is AvisoPush ? nameof(CanalAviso.Push) : nameof(CanalAviso.Correo),
        a.Estado.ToString(),
        a.Plantilla.Codigo,
        a.Destinatario.CodigoReporte,
        a.FechaCreacion,
        a.FechaEnvio);

    public static AvisoDetalle Detalle(Aviso a) => new(
        a.Id,
        a.Canal.ToString(),
        a.Estado.ToString(),
        a.Plantilla.Codigo,
        a.Destinatario.CodigoReporte,
        a switch { AvisoPush p => p.TokenDispositivo, AvisoCorreo c => c.Correo, _ => "" },
        (a as AvisoCorreo)?.Asunto,
        a.Mensaje,
        a.FechaCreacion,
        a.FechaEnvio,
        a.Intentos.OrderBy(i => i.Fecha).Select(i => new IntentoSalida(i.Id, i.Fecha, i.Exitoso, i.Detalle)).ToList());
}
