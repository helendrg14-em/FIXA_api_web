using System.Net.Mail;
using Fixa.Notificaciones.Modelos;

namespace Fixa.Notificaciones.Servicios;

/// <summary>Resultado de intentar entregar un aviso.</summary>
public sealed record ResultadoEnvio(bool Exitoso, string Detalle);

/// <summary>
/// Un canal sabe entregar su tipo de aviso. En producción serían Firebase y Mailgun; aquí se
/// simulan para que la API funcione sin credenciales y las pruebas sean repetibles.
/// </summary>
public interface ICanalEnvio
{
    CanalAviso Canal { get; }

    Task<ResultadoEnvio> EnviarAsync(Aviso aviso, CancellationToken cancelacion);
}

/// <summary>Push simulado: un token de Firebase real tiene al menos 20 caracteres.</summary>
public sealed class CanalPushSimulado : ICanalEnvio
{
    public CanalAviso Canal => CanalAviso.Push;

    public Task<ResultadoEnvio> EnviarAsync(Aviso aviso, CancellationToken cancelacion) =>
        Task.FromResult(aviso is AvisoPush { TokenDispositivo.Length: >= 20 }
            ? new ResultadoEnvio(true, "Notificación push entregada al dispositivo.")
            : new ResultadoEnvio(false, "El token del dispositivo no es válido."));
}

/// <summary>Correo simulado: se entrega si la dirección tiene un formato válido.</summary>
public sealed class CanalCorreoSimulado : ICanalEnvio
{
    public CanalAviso Canal => CanalAviso.Correo;

    public Task<ResultadoEnvio> EnviarAsync(Aviso aviso, CancellationToken cancelacion) =>
        Task.FromResult(aviso is AvisoCorreo correo && MailAddress.TryCreate(correo.Correo, out _)
            ? new ResultadoEnvio(true, $"Correo entregado a {correo.Correo}.")
            : new ResultadoEnvio(false, "La dirección de correo no es válida."));
}
