namespace Fixa.Notificaciones.Modelos;

public class IntentoEntrega
{
    public int Id { get; set; }

    public int AvisoId { get; set; }

    public Aviso Aviso { get; set; } = null!;

    public DateTime Fecha { get; set; }

    public bool Exitoso { get; set; }

    public string Detalle { get; set; } = null!;
}
