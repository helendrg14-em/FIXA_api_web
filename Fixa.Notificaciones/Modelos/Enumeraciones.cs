namespace Fixa.Notificaciones.Modelos;

/// <summary>Medio por el que se envía un aviso al vecino.</summary>
public enum CanalAviso
{
    Push,
    Correo,
}

/// <summary>Situación de un aviso: se crea pendiente y queda enviado o fallido tras cada intento.</summary>
public enum EstadoAviso
{
    Pendiente,
    Enviado,
    Fallido,
}
