using Fixa.Notificaciones.Datos;
using Fixa.Notificaciones.Dtos;
using Fixa.Notificaciones.Modelos;
using Fixa.Notificaciones.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fixa.Notificaciones.Controladores;


[ApiController]
[Route("api/v1/avisos")]
[Produces("application/json")]
public sealed class AvisosController(NotificacionesDbContext db, ServicioAvisos servicio) : ControllerBase
{

    [HttpGet]
    [EndpointSummary("Listar avisos")]
    [ProducesResponseType<List<AvisoResumen>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<AvisoResumen>>> Listar(
        [FromQuery] string? codigoReporte, [FromQuery] EstadoAviso? estado, CancellationToken cancelacion)
    {
        var consulta = db.Avisos.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(codigoReporte))
            consulta = consulta.Where(a => a.Destinatario.CodigoReporte == codigoReporte.Trim());
        if (estado is not null)
            consulta = consulta.Where(a => a.Estado == estado);

        return await consulta.OrderByDescending(a => a.Id).Select(ProyeccionesAviso.Resumen).ToListAsync(cancelacion);
    }


    [HttpGet("{id:int}")]
    [EndpointSummary("Consultar un aviso por ID")]
    [ProducesResponseType<AvisoDetalle>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AvisoDetalle>> ObtenerPorId(int id, CancellationToken cancelacion)
    {
        var aviso = await db.Avisos.AsNoTracking()
            .Include(a => a.Plantilla).Include(a => a.Destinatario).Include(a => a.Intentos)
            .FirstOrDefaultAsync(a => a.Id == id, cancelacion);

        return aviso is null ? NoEncontrado() : ProyeccionesAviso.Detalle(aviso);
    }


    [HttpPost]
    [EndpointSummary("Crear y enviar un aviso")]
    [ProducesResponseType<AvisoDetalle>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AvisoDetalle>> Crear(AvisoEntrada entrada, CancellationToken cancelacion)
    {
        try
        {
            var aviso = await servicio.CrearYEnviarAsync(entrada, cancelacion);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = aviso.Id }, await DetalleAsync(aviso.Id, cancelacion));
        }
        catch (ErrorDeDatos error)
        {
            return DatosInvalidos(error);
        }
    }

    [HttpPost("{id:int}/reintentos")]
    [EndpointSummary("Reintentar un aviso fallido")]
    [ProducesResponseType<AvisoDetalle>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AvisoDetalle>> Reintentar(int id, CancellationToken cancelacion)
    {
        try
        {
            var aviso = await servicio.ReintentarAsync(id, cancelacion);
            return aviso is null ? NoEncontrado() : await DetalleAsync(id, cancelacion);
        }
        catch (ErrorDeDatos error)
        {
            return DatosInvalidos(error);
        }
    }

    private async Task<AvisoDetalle> DetalleAsync(int id, CancellationToken cancelacion) =>
        ProyeccionesAviso.Detalle(await db.Avisos.AsNoTracking()
            .Include(a => a.Plantilla).Include(a => a.Destinatario).Include(a => a.Intentos)
            .FirstAsync(a => a.Id == id, cancelacion));

    private ObjectResult NoEncontrado() =>
        Problem(statusCode: StatusCodes.Status404NotFound, title: "El aviso no existe.");

    private ActionResult DatosInvalidos(ErrorDeDatos error)
    {
        ModelState.AddModelError(error.Campo, error.Message);
        return ValidationProblem(title: "Revisa los datos enviados.", modelStateDictionary: ModelState);
    }
}
