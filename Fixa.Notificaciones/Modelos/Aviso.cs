namespace Fixa.Notificaciones.Modelos;

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

    public ICollection<IntentoEntrega> Intentos { get; set; } = new List<IntentoEntrega>();

    public abstract CanalAviso Canal { get; }
}

public class AvisoPush : Aviso
{
    public string TokenDispositivo { get; set; } = null!;

    public string Titulo { get; set; } = null!;

    public override CanalAviso Canal => CanalAviso.Push;
}

public class AvisoCorreo : Aviso
{
    public string Correo { get; set; } = null!;

    public string Asunto { get; set; } = null!;

    public override CanalAviso Canal => CanalAviso.Correo;
}
