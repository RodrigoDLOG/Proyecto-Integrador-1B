using System.Text.RegularExpressions;
using SistemaVenta.Entity;

namespace SistemaVenta.BLL.Implementacion
{
    // Reglas del SAT que comparten Negocio y Cliente. Si algo no cumple se lanza
    // TaskCanceledException con el motivo, igual que el resto de los servicios del tutorial.
    public static class ValidacionesSat
    {
        public const string RfcPublicoGeneral = "XAXX010101000";

        // Persona moral: 3 letras + fecha AAMMDD + homoclave (12). Persona fisica: 4 letras (13).
        private static readonly Regex FormatoRfc = new(@"^[A-ZÑ&]{3,4}[0-9]{6}[A-Z0-9]{3}$");
        private static readonly Regex FormatoCodigoPostal = new(@"^[0-9]{5}$");

        // Regresa el RFC en mayusculas y sin espacios
        public static string ValidarRfc(string? rfc)
        {
            string limpio = (rfc ?? "").Trim().ToUpper();
            if (!FormatoRfc.IsMatch(limpio))
                throw new TaskCanceledException("El RFC no es valido: deben ser 12 caracteres (persona moral) o 13 (persona fisica)");
            return limpio;
        }

        public static string ValidarCodigoPostal(string? codigoPostal)
        {
            string limpio = (codigoPostal ?? "").Trim();
            if (!FormatoCodigoPostal.IsMatch(limpio))
                throw new TaskCanceledException("El codigo postal debe tener 5 digitos");
            return limpio;
        }

        // El catalogo dice si cada regimen aplica a persona fisica, moral o ambas
        public static void ValidarRegimen(CRegimenFiscalSat? regimen, string rfc)
        {
            if (regimen == null)
                throw new TaskCanceledException("Seleccione un regimen fiscal valido");

            bool esMoral = rfc.Length == 12;
            string aplica = esMoral ? regimen.Moral ?? "" : regimen.Fisica ?? "";
            if (!aplica.StartsWith("S"))
                throw new TaskCanceledException(
                    $"El regimen {regimen.CRegimenFiscal} no aplica para persona {(esMoral ? "moral" : "fisica")} (RFC de {rfc.Length} caracteres)");
        }
    }
}
