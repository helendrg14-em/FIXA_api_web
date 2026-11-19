namespace Fixa.Notificaciones.Modelos;

/// <summary>
/// Texto base de un aviso. El cuerpo admite los marcadores {codigo} y {estado}, que se
/// reemplazan con el código del reporte y el estado del caso al crear cada aviso.
/// </summary>
public class Plantilla
{
    public int Id { get; set; }

    /// <summary>Identificador estable que usa FIXA para pedir el aviso, por ejemplo "caso_cerrado".</summary>
    public string Codigo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public CanalAviso Canal { get; set; }

    /// <summary>Obligatorio cuando el canal es correo.</summary>
    public string? Asunto { get; set; }

    public string Cuerpo { get; set; } = null!;

    public bool Activa { get; set; } = true;

    public ICollection<Aviso> Avisos { get; set; } = new List<Aviso>();
}
