namespace SistemaVenta.AplicacionWeb.Models.ViewModels
{
    // Ventana Solicitar Factura (diapositiva 29) y su resultado
    public class VMFactura
    {
        public string? NumeroVenta { get; set; }
        public string? UsoCfdi { get; set; }
        public string? RegimenFiscalReceptor { get; set; }
        public string? FormaPago { get; set; }
        public string? TipoComprobante { get; set; }
        public string? MetodoPago { get; set; }
        public string? CodigoPostal { get; set; }
        // Lo que regresa el timbrado
        public string? Uuid { get; set; }
        public string? Folio { get; set; }
        public string? FechaTimbrado { get; set; }
        public string? MensajeError { get; set; }
    }
}
