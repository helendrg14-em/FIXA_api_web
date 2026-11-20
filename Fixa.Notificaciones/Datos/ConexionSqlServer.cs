using Microsoft.Data.SqlClient;

namespace Fixa.Notificaciones.Datos;

/// <summary>
/// Encuentra el SQL Server local de la máquina donde se ejecuta el proyecto. Si la cadena apunta
/// a "localhost" y ahí no hay servidor, prueba las instalaciones más comunes (SQL Express y
/// LocalDB), así el proyecto funciona en otra PC sin tocar appsettings.json.
/// </summary>
public static class ConexionSqlServer
{
    private static readonly string[] ServidoresLocales = ["localhost", ".", "(local)"];
    private static readonly string[] Alternativas = [@".\SQLEXPRESS", @"(localdb)\MSSQLLocalDB"];

    public static string Resolver(string cadena, out string servidor)
    {
        var original = new SqlConnectionStringBuilder(cadena);
        servidor = original.DataSource;

        // Una cadena a otro servidor (o con instancia propia) se respeta tal cual.
        if (!ServidoresLocales.Contains(original.DataSource, StringComparer.OrdinalIgnoreCase) || Responde(original))
            return cadena;

        foreach (var alternativa in Alternativas)
        {
            var prueba = new SqlConnectionStringBuilder(cadena) { DataSource = alternativa };
            if (Responde(prueba))
            {
                servidor = alternativa;
                return prueba.ConnectionString;
            }
        }

        return cadena; // Ninguno responde: el error claro sale al aplicar las migraciones.
    }

    private static bool Responde(SqlConnectionStringBuilder cadena)
    {
        // Se prueba contra "master" porque la base del proyecto puede no existir todavía.
        var prueba = new SqlConnectionStringBuilder(cadena.ConnectionString) { InitialCatalog = "master", ConnectTimeout = 5 };
        try
        {
            using var conexion = new SqlConnection(prueba.ConnectionString);
            conexion.Open();
            return true;
        }
        catch (SqlException)
        {
            return false;
        }
    }
}
