using System;
using System.Collections.Generic;

namespace SistemaVenta.Entity;

public partial class DetalleVenta
{
    public int IdDetalleVenta { get; set; }

    public int? IdVenta { get; set; }

    public int? IdProducto { get; set; }

    public string? MarcaProducto { get; set; }

    public string? DescripcionProducto { get; set; }

    public string? CategoriaProducto { get; set; }

    public int? Cantidad { get; set; }

    public decimal? Precio { get; set; }

    public decimal? Total { get; set; }

    // Version Mexico (diapositivas 26 y 27). Total = SubTotal + Impuesto.
    public decimal? PorcentajeDescuento { get; set; }

    public decimal? Descuento { get; set; }

    public decimal? SubTotal { get; set; }

    public decimal? TasaImpuesto { get; set; }

    public decimal? Impuesto { get; set; }

    // Claves SAT copiadas del producto al vender (para la factura)
    public string? ClaveProdServ { get; set; }

    public string? ClaveUnidad { get; set; }

    public string? Unidad { get; set; }

    public string? ObjetoImp { get; set; }

    public string? ClaveImpuesto { get; set; }

    public virtual Venta? IdVentaNavigation { get; set; }
}
