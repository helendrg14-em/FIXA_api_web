using System.ComponentModel.DataAnnotations;

namespace Fixa.Notificaciones.Dtos;

public sealed class PlantillaEntrada : IValidatableObject
{
    [Required(ErrorMessage = "El código es obligatorio.")]
    [StringLength(40, ErrorMessage = "El código puede tener hasta 40 caracteres.")]
    [RegularExpression("^[a-z][a-z0-9_]*$", ErrorMessage = "El código debe empezar con una letra minúscula y solo puede contener minúsculas, números y guiones bajos.")]
    public string Codigo { get; init; } = "";

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(80, ErrorMessage = "El nombre puede tener hasta 80 caracteres.")]
    public string Nombre { get; init; } = "";

    [Required(ErrorMessage = "El canal es obligatorio.")]
    [RegularExpression("^(Push|Correo)$", ErrorMessage = "El canal debe ser Push o Correo.")]
    public string Canal { get; init; } = "";

    [StringLength(120, ErrorMessage = "El asunto puede tener hasta 120 caracteres.")]
    public string? Asunto { get; init; }

    [Required(ErrorMessage = "El cuerpo es obligatorio.")]
    [StringLength(500, ErrorMessage = "El cuerpo puede tener hasta 500 caracteres.")]
    public string Cuerpo { get; init; } = "";

    public bool Activa { get; init; } = true;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Canal == "Correo" && string.IsNullOrWhiteSpace(Asunto))
            yield return new ValidationResult("El asunto es obligatorio para una plantilla de correo.", [nameof(Asunto)]);
    }
}

public sealed record PlantillaSalida(
    int Id, string Codigo, string Nombre, string Canal, string? Asunto, string Cuerpo, bool Activa, int CantidadAvisos);
