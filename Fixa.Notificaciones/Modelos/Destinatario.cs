namespace Fixa.Notificaciones.Modelos;

/// <summary>
/// Vecino que recibe los avisos. FIXA no pide cuenta al ciudadano, así que se le identifica
/// por el código de su reporte (FX-AAAA-000000) y no por nombre ni documento.
/// </summary>
public class Destinatario
{
    public int Id { get; set; }

    public string CodigoReporte { get; set; } = null!;

    public string? Correo { get; set; }

    public string? TokenDispositivo { get; set; }

    public DateTime FechaRegistro { get; set; }

    public ICollection<Aviso> Avisos { get; set; } = new List<Aviso>();
}
