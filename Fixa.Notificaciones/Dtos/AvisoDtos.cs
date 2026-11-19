using System.ComponentModel.DataAnnotations;

namespace Fixa.Notificaciones.Dtos;

/// <summary>Lo que envía FIXA para avisar al vecino de un cambio en su caso.</summary>
public sealed class AvisoEntrada
{
    /// <example>caso_recibido</example>
    [Required(ErrorMessage = "El código de la plantilla es obligatorio.")]
    [StringLength(40, ErrorMessage = "El código de la plantilla admite hasta 40 caracteres.")]
    public string CodigoPlantilla { get; init; } = null!;

    /// <example>FX-2026-000001</example>
    [Required(ErrorMessage = "El código del reporte es obligatorio.")]
    [RegularExpression(@"^FX-\d{4}-\d{6}$", ErrorMessage = "El código del reporte debe tener el formato FX-AAAA-000000.")]
    public string CodigoReporte { get; init; } = null!;

    /// <example>Recibida</example>
    [Required(ErrorMessage = "El estado del caso es obligatorio.")]
    [StringLength(40, ErrorMessage = "El estado admite hasta 40 caracteres.")]
    public string EstadoCaso { get; init; } = null!;
}

/// <summary>Resumen de un aviso, para listas.</summary>
public sealed record AvisoResumen(
    int Id,
    string Canal,
    string Estado,
    string CodigoPlantilla,
    string CodigoReporte,
    DateTime FechaCreacion,
    DateTime? FechaEnvio);

/// <summary>Un intento de entrega dentro del detalle de un aviso.</summary>
public sealed record IntentoSalida(int Id, DateTime Fecha, bool Exitoso, string Detalle);

/// <summary>Detalle completo de un aviso, con el destino según su canal y sus intentos.</summary>
public sealed record AvisoDetalle(
    int Id,
    string Canal,
    string Estado,
    string CodigoPlantilla,
    string CodigoReporte,
    string Destino,
    string? Asunto,
    string Mensaje,
    DateTime FechaCreacion,
    DateTime? FechaEnvio,
    IReadOnlyList<IntentoSalida> Intentos);
