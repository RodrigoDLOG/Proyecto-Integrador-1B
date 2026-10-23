namespace SistemaVenta.AplicacionWeb.Models.ViewModels
{
    public class VMDetalleVenta
    {
        public int? IdProducto { get; set; }
        public string? MarcaProducto { get; set; }
        public string? DescripcionProducto { get; set; }
        public string? CategoriaProducto { get; set; }
        public int? Cantidad { get; set; }
        public string? Precio { get; set; }
        public string? Total { get; set; }
        // Version Mexico (diapositiva 26): descuento e impuesto por producto
        public string? PorcentajeDescuento { get; set; }
        public string? Descuento { get; set; }
        public string? SubTotal { get; set; }
        public string? TasaImpuesto { get; set; }
        public string? Impuesto { get; set; }
    }
}
