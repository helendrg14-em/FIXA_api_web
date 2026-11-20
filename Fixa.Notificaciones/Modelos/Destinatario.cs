namespace Fixa.Notificaciones.Modelos;

public class Destinatario
{
    public int Id { get; set; }

    public string CodigoReporte { get; set; } = null!;

    public string? Correo { get; set; }

    public string? TokenDispositivo { get; set; }

    public DateTime FechaRegistro { get; set; }

    public ICollection<Aviso> Avisos { get; set; } = new List<Aviso>();
}
