using System.Text.Json.Serialization;
using Fixa.Notificaciones.Datos;
using Fixa.Notificaciones.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// La cadena está en appsettings.json y usa autenticación de Windows, sin contraseñas.
// Si el SQL Server local no está en "localhost", se busca en SQL Express y LocalDB.
var conexion = ConexionSqlServer.Resolver(
    builder.Configuration.GetConnectionString("Notificaciones")
        ?? throw new InvalidOperationException("Falta ConnectionStrings:Notificaciones en appsettings.json."),
    out var servidorSql);

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

// Crea la base, sus tablas y los datos de prueba al iniciar (si ya existen, no hace nada).
using (var alcance = app.Services.CreateScope())
{
    try
    {
        alcance.ServiceProvider.GetRequiredService<NotificacionesDbContext>().Database.Migrate();
        app.Logger.LogInformation("Base de datos lista en el servidor {Servidor}.", servidorSql);
    }
    catch (Microsoft.Data.SqlClient.SqlException)
    {
        app.Logger.LogCritical(
            "No se pudo conectar a SQL Server ({Servidor}). Revisa que el servicio esté iniciado " +
            "o cambia ConnectionStrings:Notificaciones en appsettings.json.", servidorSql);
        throw;
    }
}

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    // SWAGGER: lo agrega Kevin (UseSwagger y UseSwaggerUI).
}

app.MapControllers();
app.Run();
