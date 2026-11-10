using Microsoft.EntityFrameworkCore.Migrations;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Domain.Migrations.Scripts
{
    /// <summary>
    /// Ejecuta desde una migración los archivos .sql embebidos en Domain/Migrations/Scripts.
    /// Los scripts se separan por líneas "GO" (igual que en SSMS), porque GO no es T-SQL.
    ///
    /// Regla: un script ya aplicado NUNCA se edita. Para cambiar un SP se crea un archivo
    /// nuevo (p. ej. V002_Compra.sql) y una migración nueva que lo ejecute.
    /// </summary>
    public static class ScriptRunner
    {
        private static readonly Regex SeparadorGo =
            new(@"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static void EjecutarScript(this MigrationBuilder migrationBuilder, string nombreArchivo)
        {
            var assembly = typeof(ScriptRunner).Assembly;
            var recurso = assembly.GetManifestResourceNames()
                .SingleOrDefault(r => r.EndsWith($".Migrations.Scripts.{nombreArchivo}", StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException($"No se encontró el script embebido '{nombreArchivo}'.");

            using var stream = assembly.GetManifestResourceStream(recurso)!;
            using var reader = new StreamReader(stream);
            var contenido = reader.ReadToEnd();

            foreach (var lote in SeparadorGo.Split(contenido))
            {
                if (!string.IsNullOrWhiteSpace(lote))
                    migrationBuilder.Sql(lote);
            }
        }
    }
}
