using System.Linq.Expressions;
using Fixa.Notificaciones.Datos;
using Fixa.Notificaciones.Dtos;
using Fixa.Notificaciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Fixa.Notificaciones.Controladores;

[ApiController]
[Route("api/v1/destinatarios")]
[Produces("application/json")]
public sealed class DestinatariosController(NotificacionesDbContext db) : ControllerBase
{
    private const string MensajeCodigoRepetido = "Ya existe un destinatario con ese código de reporte.";

    private static readonly Expression<Func<Destinatario, DestinatarioSalida>> Proyectar = destinatario =>
        new DestinatarioSalida(destinatario.Id, destinatario.CodigoReporte, destinatario.Correo,
            destinatario.TokenDispositivo, destinatario.FechaRegistro);

    [HttpGet]
    [EndpointSummary("Listar destinatarios")]
    [ProducesResponseType<List<DestinatarioSalida>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DestinatarioSalida>>> Listar(CancellationToken cancelacion) =>
        await db.Destinatarios.AsNoTracking().OrderBy(destinatario => destinatario.Id)
            .Select(Proyectar).ToListAsync(cancelacion);

    [HttpGet("{id:int}")]
    [EndpointSummary("Consultar un destinatario por ID")]
    [ProducesResponseType<DestinatarioSalida>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DestinatarioSalida>> ObtenerPorId(int id, CancellationToken cancelacion)
    {
        var destinatario = await db.Destinatarios.AsNoTracking().Where(destinatario => destinatario.Id == id)
            .Select(Proyectar).SingleOrDefaultAsync(cancelacion);

        return destinatario is null ? NoEncontrado() : destinatario;
    }

    [HttpPost]
    [EndpointSummary("Crear un destinatario")]
    [ProducesResponseType<DestinatarioSalida>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DestinatarioSalida>> Crear(DestinatarioEntrada entrada, CancellationToken cancelacion)
    {
        if (await db.Destinatarios.AnyAsync(destinatario => destinatario.CodigoReporte == entrada.CodigoReporte, cancelacion))
            return ConflictoCodigo();

        var destinatario = new Destinatario();
        AplicarDatos(destinatario, entrada);
        db.Destinatarios.Add(destinatario);

        try
        {
            await db.SaveChangesAsync(cancelacion);
        }
        catch (DbUpdateException error) when (error.InnerException is SqlException { Number: 2601 or 2627 })
        {
            return ConflictoCodigo();
        }

        var salida = new DestinatarioSalida(destinatario.Id, destinatario.CodigoReporte, destinatario.Correo,
            destinatario.TokenDispositivo, destinatario.FechaRegistro);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = destinatario.Id }, salida);
    }

    [HttpPut("{id:int}")]
    [EndpointSummary("Actualizar un destinatario")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(int id, DestinatarioEntrada entrada, CancellationToken cancelacion)
    {
        var destinatario = await db.Destinatarios.FindAsync([id], cancelacion);
        if (destinatario is null)
            return NoEncontrado();

        if (await db.Destinatarios.AnyAsync(otro => otro.Id != id && otro.CodigoReporte == entrada.CodigoReporte, cancelacion))
            return CodigoInvalido();

        AplicarDatos(destinatario, entrada);
        try
        {
            await db.SaveChangesAsync(cancelacion);
        }
        catch (DbUpdateConcurrencyException)
        {
            return NoEncontrado();
        }
        catch (DbUpdateException error) when (error.InnerException is SqlException { Number: 2601 or 2627 })
        {
            return CodigoInvalido();
        }

        return NoContent();
    }

    [HttpGet("{id:int}/avisos")]
    [EndpointSummary("Listar los avisos de un destinatario")]
    [ProducesResponseType<List<AvisoResumen>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<AvisoResumen>>> ListarAvisos(int id, CancellationToken cancelacion)
    {
        if (!await db.Destinatarios.AnyAsync(destinatario => destinatario.Id == id, cancelacion))
            return NoEncontrado();

        return await db.Avisos.AsNoTracking().Where(aviso => aviso.DestinatarioId == id)
            .OrderByDescending(aviso => aviso.Id).Select(ProyeccionesAviso.Resumen).ToListAsync(cancelacion);
    }

    private static void AplicarDatos(Destinatario destinatario, DestinatarioEntrada entrada)
    {
        destinatario.CodigoReporte = entrada.CodigoReporte;
        destinatario.Correo = string.IsNullOrWhiteSpace(entrada.Correo) ? null : entrada.Correo.Trim();
        destinatario.TokenDispositivo = string.IsNullOrWhiteSpace(entrada.TokenDispositivo) ? null : entrada.TokenDispositivo.Trim();
    }

    private ObjectResult NoEncontrado() =>
        Problem(statusCode: StatusCodes.Status404NotFound, title: "El destinatario no existe.");

    private ObjectResult ConflictoCodigo() =>
        Problem(statusCode: StatusCodes.Status409Conflict, title: MensajeCodigoRepetido);

    private ActionResult CodigoInvalido()
    {
        ModelState.AddModelError("codigoReporte", MensajeCodigoRepetido);
        return ValidationProblem(title: "Revisa los datos enviados.", modelStateDictionary: ModelState);
    }
}
