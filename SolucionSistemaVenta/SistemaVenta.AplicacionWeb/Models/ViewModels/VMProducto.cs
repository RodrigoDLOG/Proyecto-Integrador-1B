namespace SistemaVenta.AplicacionWeb.Models.ViewModels
{
    public class VMProducto
    {
        public int IdProducto { get; set; }
        public string? CodigoBarra { get; set; }
        public string? Marca { get; set; }
        public string? Descripcion { get; set; }
        public int? IdCategoria { get; set; }
        public string? NombreCategoria { get; set; }
        public int? Stock { get; set; }
        public string? UrlImagen { get; set; }
        public string? Precio { get; set; }
        public int? EsActivo { get; set; }

        // Version Mexico (diapositiva 22). Precio = Precio Unitario sin IVA; Descuento y TasaImpuesto en %.
        public int? IdMedidaLocal { get; set; }
        public string? NombreMedidaLocal { get; set; }
        public string? ClaveUnidad { get; set; }
        public string? NombreClaveUnidad { get; set; }
        public string? ClaveProdServ { get; set; }
        public string? DescripcionClaveProdServ { get; set; }
        public string? Descuento { get; set; }
        public string? ObjetoImp { get; set; }
        public string? ClaveImpuesto { get; set; }
        public string? TasaImpuesto { get; set; }
    }
}
