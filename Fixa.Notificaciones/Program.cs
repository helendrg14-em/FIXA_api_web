using System.Text.Json.Serialization;
using Fixa.Notificaciones.Datos;
using Fixa.Notificaciones.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// La cadena va en User Secrets (desarrollo) o en variables de entorno; nunca en el repositorio.
var conexion = builder.Configuration.GetConnectionString("Notificaciones")
    ?? throw new InvalidOperationException("Configura ConnectionStrings:Notificaciones (ver README).");

builder.Services.AddDbContext<NotificacionesDbContext>(opciones => opciones.UseSqlServer(conexion));

// Canales de envío (simulados) y el servicio que crea y entrega los avisos.
builder.Services.AddSingleton<ICanalEnvio, CanalPushSimulado>();
builder.Services.AddSingleton<ICanalEnvio, CanalCorreoSimulado>();
builder.Services.AddScoped<ServicioAvisos>();

builder.Services.AddControllers(mvc =>
    {
        var mensajes = mvc.ModelBindingMessageProvider;
        mensajes.SetValueMustNotBeNullAccessor(_ => "El campo es obligatorio.");
        mensajes.SetAttemptedValueIsInvalidAccessor((valor, campo) => $"El valor «{valor}» no es válido para {campo}.");
        mensajes.SetValueIsInvalidAccessor(valor => $"El valor «{valor}» no es válido.");
        mensajes.SetNonPropertyAttemptedValueIsInvalidAccessor(valor => $"El valor «{valor}» no es válido.");
        mensajes.SetMissingRequestBodyRequiredValueAccessor(() => "Faltan los datos de la petición.");
    })
    // Los enums viajan como texto ("Push", "Fallido") y no como números.
    .AddJsonOptions(json =>
    {
        json.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        json.AllowInputFormatterExceptionMessages = false;
    })
    .ConfigureApiBehaviorOptions(api => api.InvalidModelStateResponseFactory = contexto =>
    {
        // Un JSON que no se puede leer se informa con un mensaje claro, sin detalles internos.
        var errores = contexto.ModelState;
        if (errores.Keys.Any(clave => clave.StartsWith('$')))
        {
            errores = new();
            errores.AddModelError("datos", "Los datos enviados no tienen el formato JSON esperado.");
        }

        return new BadRequestObjectResult(new ValidationProblemDetails(errores)
        {
            Title = "Revisa los datos enviados.",
            Status = StatusCodes.Status400BadRequest,
        });
    });

builder.Services.AddProblemDetails();

// SWAGGER: lo agrega Kevin (AddEndpointsApiExplorer y AddSwaggerGen).

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    // SWAGGER: lo agrega Kevin (UseSwagger y UseSwaggerUI).
}

app.MapControllers();
app.Run();
