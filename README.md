# FIXA · API de Notificaciones

Web API del módulo de notificaciones de **FIXA**, el sistema de gestión de incidencias urbanas
de Santa Cruz de la Sierra. Guarda las plantillas de los mensajes, los destinatarios (vecinos
identificados por el código de su reporte) y cada aviso enviado por **push** o por **correo**,
con sus intentos de entrega.

| Dato | Valor |
|---|---|
| Materia | Programación Web III — Univalle Santa Cruz, gestión 2-2026 |
| Versión de .NET | **.NET 10** (ASP.NET Core Web API) |
| Base de datos | **SQL Server** con Entity Framework Core 10 (migraciones) |
| Documentación | Swagger UI |
| Repositorio | https://github.com/helendrg14-em/FIXA_api_web |

## Integrantes

| Nombre | Usuario de GitHub | Aporte principal |
|---|---|---|
| Helen Darlin Rios Gutierrez | [helendrg14-em](https://github.com/helendrg14-em) | Modelo de datos, generalización, configuración de EF Core, migraciones, datos de prueba, avisos y documentación |
| Kevin Fernando Illanes Rios | *(completar con el usuario de GitHub)* | DTO, CRUD de plantillas, destinatarios, Swagger UI y pruebas |

## Modelo relacional

```
Plantilla 1 ──< Aviso >── 1 Destinatario
                  │
                  ◆ composición (borrado en cascada)
                  └──< IntentoEntrega

Aviso (abstracta)            ← generalización: una tabla "Avisos" con la columna "Canal"
 ├── AvisoPush               (TokenDispositivo, Titulo)
 └── AvisoCorreo             (Correo, Asunto)
```

| Tabla | Clave | Relaciones y reglas |
|---|---|---|
| `Plantillas` | `Id` | `Codigo` único. Si el canal es correo, el asunto es obligatorio (restricción CHECK) |
| `Destinatarios` | `Id` | `CodigoReporte` único. Debe tener correo o token de dispositivo (CHECK) |
| `Avisos` | `Id` | FK a `Plantillas` y a `Destinatarios`, sin borrado en cascada: el historial se conserva. Columna discriminadora `Canal` (`Push` / `Correo`) |
| `IntentosEntrega` | `Id` | FK a `Avisos` con borrado en cascada (composición) |

## Estructura del proyecto

```
Fixa.Notificaciones/
├── Modelos/          entidades y enumeraciones
├── Datos/
│   ├── NotificacionesDbContext.cs
│   ├── Configuraciones/   una clase por entidad: claves, relaciones, herencia y datos de prueba
│   └── Migraciones/
├── Dtos/             objetos de entrada y salida de la API
├── Controladores/
├── Servicios/        creación y envío de avisos (canales push y correo simulados)
└── Program.cs        inyección de dependencias y configuración
```

## Configuración local

### 1. Requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0): `dotnet --version`
- SQL Server 2019 o superior (Developer o Express), o SQL Server en Docker
- Herramienta de EF Core: `dotnet tool install --global dotnet-ef`

### 2. Cadena de conexión

La cadena **no se guarda en el repositorio**: va en *User Secrets*. Ejemplos sin credenciales reales:

```bash
cd Fixa.Notificaciones

# SQL Server instalado en Windows (autenticación de Windows)
dotnet user-secrets set "ConnectionStrings:Notificaciones" "Server=localhost;Database=FixaNotificaciones;Trusted_Connection=True;TrustServerCertificate=True"

# SQL Server con usuario y contraseña (por ejemplo, en Docker)
dotnet user-secrets set "ConnectionStrings:Notificaciones" "Server=localhost,1433;Database=FixaNotificaciones;User Id=sa;Password=TU_CONTRASEÑA;TrustServerCertificate=True"
```

### 3. Migraciones y datos de prueba

```bash
dotnet ef database update
```

Crea la base `FixaNotificaciones`, sus 4 tablas y carga los datos de prueba:

| Datos de prueba | Para qué |
|---|---|
| Plantillas `caso_recibido` y `caso_programado` (push) y `caso_cerrado` (correo) | Crear avisos de los dos canales |
| Destinatario `FX-2026-000001` con token válido | Aviso push que se entrega |
| Destinatario `FX-2026-000002` con correo | Aviso por correo que se entrega |
| Destinatario `FX-2026-000003` con token vencido | Aviso push que falla, para probar el reintento |

### 4. Ejecutar y abrir Swagger

```bash
dotnet run
```

Swagger UI: **http://localhost:5180/swagger**

## Endpoints

| Método | Ruta | Respuestas |
|---|---|---|
| GET | `/api/v1/plantillas` | 200 |
| GET | `/api/v1/plantillas/{id}` | 200 · 404 |
| POST | `/api/v1/plantillas` | 201 + `Location` · 400 · 409 |
| PUT | `/api/v1/plantillas/{id}` | 204 · 400 · 404 · 409 |
| DELETE | `/api/v1/plantillas/{id}` | 204 · 400 si ya tiene avisos · 404 |
| GET | `/api/v1/plantillas/{id}/avisos` | 200 · 404 (recurso relacionado) |
| GET | `/api/v1/destinatarios` | 200 |
| GET | `/api/v1/destinatarios/{id}` | 200 · 404 |
| POST | `/api/v1/destinatarios` | 201 + `Location` · 400 · 409 |
| PUT | `/api/v1/destinatarios/{id}` | 204 · 400 · 404 |
| GET | `/api/v1/destinatarios/{id}/avisos` | 200 · 404 |
| GET | `/api/v1/avisos?codigoReporte=&estado=` | 200 · 400 |
| GET | `/api/v1/avisos/{id}` | 200 · 404 |
| POST | `/api/v1/avisos` | 201 + `Location` · 400 |
| POST | `/api/v1/avisos/{id}/reintentos` | 200 · 400 si no está fallido · 404 |

### Reglas del dominio

- Un aviso usa el canal de su plantilla: push necesita token de dispositivo y correo necesita
  dirección de correo. Si el destinatario no tiene el dato, la API responde 400.
- Una plantilla inexistente o desactivada, o un código de reporte sin destinatario, responden
  400 (referencia inválida).
- Si la entrega falla, el aviso queda **Fallido** con su intento registrado y se puede
  reintentar. El reintento usa el contacto actual del destinatario.
- Una plantilla con avisos enviados no se borra: se desactiva.

## Ejemplo

```http
POST http://localhost:5180/api/v1/avisos
Content-Type: application/json

{
  "codigoPlantilla": "caso_recibido",
  "codigoReporte": "FX-2026-000001",
  "estadoCaso": "Recibida"
}
```

Respuesta **201 Created** con `Location: http://localhost:5180/api/v1/avisos/1` y el aviso
creado como `AvisoPush`, en estado `Enviado`.

## Relación con FIXA

El sistema FIXA (panel Laravel, app Flutter y su API principal) trabaja hoy sobre PostgreSQL.
Este servicio se desarrolló aparte, en SQL Server, como módulo de notificaciones: más adelante
la API principal lo llamará con `POST /api/v1/avisos` cada vez que cambie el estado de un caso.
En producción, los canales simulados se reemplazan por Firebase (push) y Mailgun (correo).
