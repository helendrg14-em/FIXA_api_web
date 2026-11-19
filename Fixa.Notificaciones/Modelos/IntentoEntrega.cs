namespace Fixa.Notificaciones.Modelos;

/// <summary>Cada vez que se intenta entregar un aviso queda un registro, salga bien o mal.</summary>
public class IntentoEntrega
{
    public int Id { get; set; }

    public int AvisoId { get; set; }

    public Aviso Aviso { get; set; } = null!;

    public DateTime Fecha { get; set; }

    public bool Exitoso { get; set; }

    public string Detalle { get; set; } = null!;
}
