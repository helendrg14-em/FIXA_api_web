using System.ComponentModel.DataAnnotations;

namespace Fixa.Notificaciones.Dtos;

public sealed class DestinatarioEntrada : IValidatableObject
{
    [Required(ErrorMessage = "El código de reporte es obligatorio.")]
    [RegularExpression(@"^FX-\d{4}-\d{6}$", ErrorMessage = "El código de reporte debe tener el formato FX-AAAA-000000.")]
    public string CodigoReporte { get; init; } = "";

    [EmailAddress(ErrorMessage = "El correo debe ser una dirección válida.")]
    [StringLength(120, ErrorMessage = "El correo puede tener hasta 120 caracteres.")]
    public string? Correo { get; init; }

    [StringLength(255, ErrorMessage = "El token de dispositivo puede tener hasta 255 caracteres.")]
    public string? TokenDispositivo { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Correo) && string.IsNullOrWhiteSpace(TokenDispositivo))
            yield return new ValidationResult("Indica un correo o un token de dispositivo.", [nameof(Correo), nameof(TokenDispositivo)]);
    }
}

public sealed record DestinatarioSalida(
    int Id, string CodigoReporte, string? Correo, string? TokenDispositivo, DateTime FechaRegistro);
