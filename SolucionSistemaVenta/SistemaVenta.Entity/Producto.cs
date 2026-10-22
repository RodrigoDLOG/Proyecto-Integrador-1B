using System;
using System.Collections.Generic;

namespace SistemaVenta.Entity;

public partial class Producto
{
    public int IdProducto { get; set; }

    public string? CodigoBarra { get; set; }

    public string? Marca { get; set; }

    public string? Descripcion { get; set; }

    public int? IdCategoria { get; set; }

    public int? Stock { get; set; }

    public string? UrlImagen { get; set; }

    public string? NombreImagen { get; set; }

    public decimal? Precio { get; set; }

    public bool? EsActivo { get; set; }

    public DateTime? FechaRegistro { get; set; }

    // Version Mexico (diapositiva 22). Precio = Precio Unitario sin IVA.
    public int? IdMedidaLocal { get; set; }

    public string? ClaveUnidad { get; set; }

    public string? ClaveProdServ { get; set; }

    public decimal? Descuento { get; set; }

    public string? ObjetoImp { get; set; }

    public string? ClaveImpuesto { get; set; }

    public decimal? TasaImpuesto { get; set; }

    public virtual Categoria? IdCategoriaNavigation { get; set; }
}
