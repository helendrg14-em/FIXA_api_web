using System.Net.Mail;
using Fixa.Notificaciones.Modelos;

namespace Fixa.Notificaciones.Servicios;

public sealed record ResultadoEnvio(bool Exitoso, string Detalle);

public interface ICanalEnvio
{
    CanalAviso Canal { get; }

    Task<ResultadoEnvio> EnviarAsync(Aviso aviso, CancellationToken cancelacion);
}

public sealed class CanalPushSimulado : ICanalEnvio
{
    public CanalAviso Canal => CanalAviso.Push;

    public Task<ResultadoEnvio> EnviarAsync(Aviso aviso, CancellationToken cancelacion) =>
        Task.FromResult(aviso is AvisoPush { TokenDispositivo.Length: >= 20 }
            ? new ResultadoEnvio(true, "Notificación push entregada al dispositivo.")
            : new ResultadoEnvio(false, "El token del dispositivo no es válido."));
}

public sealed class CanalCorreoSimulado : ICanalEnvio
{
    public CanalAviso Canal => CanalAviso.Correo;

    public Task<ResultadoEnvio> EnviarAsync(Aviso aviso, CancellationToken cancelacion) =>
        Task.FromResult(aviso is AvisoCorreo correo && MailAddress.TryCreate(correo.Correo, out _)
            ? new ResultadoEnvio(true, $"Correo entregado a {correo.Correo}.")
            : new ResultadoEnvio(false, "La dirección de correo no es válida."));
}
