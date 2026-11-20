namespace Fixa.Notificaciones.Modelos;

public class Plantilla
{
    public int Id { get; set; }

    public string Codigo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public CanalAviso Canal { get; set; }

    public string? Asunto { get; set; }

    public string Cuerpo { get; set; } = null!;

    public bool Activa { get; set; } = true;

    public ICollection<Aviso> Avisos { get; set; } = new List<Aviso>();
}
