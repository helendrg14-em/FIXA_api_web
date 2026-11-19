namespace Fixa.Notificaciones.Modelos;

/// <summary>
/// Generalización: todo aviso tiene plantilla, destinatario, mensaje, estado e intentos;
/// cada subclase agrega lo propio de su canal. Se guarda en una sola tabla con la columna
/// "Canal" como discriminador (Table Per Hierarchy).
/// </summary>
public abstract class Aviso
{
    public int Id { get; set; }

    public int PlantillaId { get; set; }

    public Plantilla Plantilla { get; set; } = null!;

    public int DestinatarioId { get; set; }

    public Destinatario Destinatario { get; set; } = null!;

    public string Mensaje { get; set; } = null!;

    public EstadoAviso Estado { get; set; } = EstadoAviso.Pendiente;

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaEnvio { get; set; }

    /// <summary>Composición: los intentos solo existen dentro de su aviso.</summary>
    public ICollection<IntentoEntrega> Intentos { get; set; } = new List<IntentoEntrega>();

    public abstract CanalAviso Canal { get; }
}

/// <summary>Aviso al celular del vecino (Firebase Cloud Messaging en producción).</summary>
public class AvisoPush : Aviso
{
    public string TokenDispositivo { get; set; } = null!;

    public string Titulo { get; set; } = null!;

    public override CanalAviso Canal => CanalAviso.Push;
}

/// <summary>Aviso por correo electrónico (Mailgun en producción).</summary>
public class AvisoCorreo : Aviso
{
    public string Correo { get; set; } = null!;

    public string Asunto { get; set; } = null!;

    public override CanalAviso Canal => CanalAviso.Correo;
}
