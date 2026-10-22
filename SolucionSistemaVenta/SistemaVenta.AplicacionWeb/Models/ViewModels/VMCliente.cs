namespace SistemaVenta.AplicacionWeb.Models.ViewModels
{
    public class VMCliente
    {
        public int IdCliente { get; set; }
        public string? Nombre { get; set; }
        public string? Correo { get; set; }
        public string? Rfc { get; set; }
        public string? DomicilioFiscalReceptor { get; set; }
        public string? RegimenFiscalReceptor { get; set; }
        public int EsActivo { get; set; }
    }
}
