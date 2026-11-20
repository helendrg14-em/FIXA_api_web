using System.ComponentModel.DataAnnotations;

namespace Fixa.Notificaciones.Dtos;

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

public sealed record AvisoResumen(
    int Id,
    string Canal,
    string Estado,
    string CodigoPlantilla,
    string CodigoReporte,
    DateTime FechaCreacion,
    DateTime? FechaEnvio);


public sealed record IntentoSalida(int Id, DateTime Fecha, bool Exitoso, string Detalle);


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
