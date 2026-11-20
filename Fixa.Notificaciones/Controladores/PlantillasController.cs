using System.Linq.Expressions;
using Fixa.Notificaciones.Datos;
using Fixa.Notificaciones.Dtos;
using Fixa.Notificaciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Fixa.Notificaciones.Controladores;

[ApiController]
[Route("api/v1/plantillas")]
[Produces("application/json")]
public sealed class PlantillasController(NotificacionesDbContext db) : ControllerBase
{
    private static readonly Expression<Func<Plantilla, PlantillaSalida>> Proyectar = plantilla =>
        new PlantillaSalida(plantilla.Id, plantilla.Codigo, plantilla.Nombre, plantilla.Canal.ToString(),
            plantilla.Asunto, plantilla.Cuerpo, plantilla.Activa, plantilla.Avisos.Count);

    [HttpGet]
    [EndpointSummary("Listar plantillas")]
    [ProducesResponseType<List<PlantillaSalida>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PlantillaSalida>>> Listar(CancellationToken cancelacion) =>
        await db.Plantillas.AsNoTracking().OrderBy(plantilla => plantilla.Id).Select(Proyectar).ToListAsync(cancelacion);

    [HttpGet("{id:int}")]
    [EndpointSummary("Consultar una plantilla por ID")]
    [ProducesResponseType<PlantillaSalida>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlantillaSalida>> ObtenerPorId(int id, CancellationToken cancelacion)
    {
        var plantilla = await db.Plantillas.AsNoTracking().Where(plantilla => plantilla.Id == id)
            .Select(Proyectar).SingleOrDefaultAsync(cancelacion);

        return plantilla is null ? NoEncontrada() : plantilla;
    }

    [HttpPost]
    [EndpointSummary("Crear una plantilla")]
    [ProducesResponseType<PlantillaSalida>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PlantillaSalida>> Crear(PlantillaEntrada entrada, CancellationToken cancelacion)
    {
        if (await db.Plantillas.AnyAsync(plantilla => plantilla.Codigo == entrada.Codigo, cancelacion))
            return CodigoRepetido();

        var plantilla = new Plantilla();
        AplicarDatos(plantilla, entrada);
        db.Plantillas.Add(plantilla);

        try
        {
            await db.SaveChangesAsync(cancelacion);
        }
        catch (DbUpdateException error) when (error.InnerException is SqlException { Number: 2601 or 2627 })
        {
            return CodigoRepetido();
        }

        var salida = new PlantillaSalida(plantilla.Id, plantilla.Codigo, plantilla.Nombre, entrada.Canal,
            plantilla.Asunto, plantilla.Cuerpo, plantilla.Activa, 0);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = plantilla.Id }, salida);
    }

    [HttpPut("{id:int}")]
    [EndpointSummary("Actualizar una plantilla")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Actualizar(int id, PlantillaEntrada entrada, CancellationToken cancelacion)
    {
        var plantilla = await db.Plantillas.FindAsync([id], cancelacion);
        if (plantilla is null)
            return NoEncontrada();

        if (await db.Plantillas.AnyAsync(otra => otra.Id != id && otra.Codigo == entrada.Codigo, cancelacion))
            return CodigoRepetido();

        AplicarDatos(plantilla, entrada);
        try
        {
            await db.SaveChangesAsync(cancelacion);
        }
        catch (DbUpdateConcurrencyException)
        {
            return NoEncontrada();
        }
        catch (DbUpdateException error) when (error.InnerException is SqlException { Number: 2601 or 2627 })
        {
            return CodigoRepetido();
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [EndpointSummary("Eliminar una plantilla sin avisos")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancelacion)
    {
        var plantilla = await db.Plantillas.FindAsync([id], cancelacion);
        if (plantilla is null)
            return NoEncontrada();

        if (await db.Avisos.AnyAsync(aviso => aviso.PlantillaId == id, cancelacion))
            return PlantillaEnUso();

        db.Plantillas.Remove(plantilla);
        try
        {
            await db.SaveChangesAsync(cancelacion);
        }
        catch (DbUpdateConcurrencyException)
        {
            return NoEncontrada();
        }
        catch (DbUpdateException error) when (error.InnerException is SqlException { Number: 547 })
        {
            return PlantillaEnUso();
        }

        return NoContent();
    }

    private static void AplicarDatos(Plantilla plantilla, PlantillaEntrada entrada)
    {
        plantilla.Codigo = entrada.Codigo;
        plantilla.Nombre = entrada.Nombre;
        plantilla.Canal = Enum.Parse<CanalAviso>(entrada.Canal);
        plantilla.Asunto = string.IsNullOrWhiteSpace(entrada.Asunto) ? null : entrada.Asunto;
        plantilla.Cuerpo = entrada.Cuerpo;
        plantilla.Activa = entrada.Activa;
    }

    private ObjectResult NoEncontrada() =>
        Problem(statusCode: StatusCodes.Status404NotFound, title: "La plantilla no existe.");

    private ObjectResult CodigoRepetido() =>
        Problem(statusCode: StatusCodes.Status409Conflict, title: "Ya existe una plantilla con ese código.");

    private ActionResult PlantillaEnUso()
    {
        ModelState.AddModelError("plantilla", "La plantilla ya se usó en avisos; desactívala en lugar de borrarla.");
        return ValidationProblem(title: "Revisa los datos enviados.", modelStateDictionary: ModelState);
    }
}
