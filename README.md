# FIXA · API de Notificaciones

- **Versión de .NET:** .NET 10
- **Repositorio:** https://github.com/helendrg14-em/FIXA_api_web

## Integrantes

| Nombre | Usuario de GitHub |
|---|---|
| Helen Darlin Rios Gutierrez | helendrg14-em |
| Kevin Fernando Illanes Rios | *(completar)* |

## 1. Conexión a SQL Server local

La cadena está en `Fixa.Notificaciones/appsettings.json` y usa autenticación de Windows. Si
SQL Server no está en `localhost`, la API lo busca sola en `.\SQLEXPRESS` y en
`(localdb)\MSSQLLocalDB`, así que normalmente no hay que cambiar nada.

Ejemplo de cadena de conexión (sin credenciales reales):

```
Server=localhost;Database=FixaNotificaciones;User Id=usuario;Password=contraseña;TrustServerCertificate=True
```

## 2. Aplicar las migraciones

Se aplican solas al ejecutar la API. Para aplicarlas a mano:

```bash
cd Fixa.Notificaciones
dotnet ef database update
```

## 3. Cargar los datos de prueba

Los datos de prueba están en la migración, así que se cargan en el paso anterior: tres
plantillas (`caso_recibido`, `caso_programado` y `caso_cerrado`) y tres destinatarios
(`FX-2026-000001`, `FX-2026-000002` y `FX-2026-000003`).

## 4. Abrir Swagger

```bash
cd Fixa.Notificaciones
dotnet run
```

Abrir **http://localhost:5180/swagger**
