using System;
using System.Collections.Generic;

namespace SistemaVenta.Entity;

// Una factura por venta (diapositivas 28 y 29): lo que se pide al solicitarla y lo que regresa el timbrado
public partial class Factura
{
    public int IdFactura { get; set; }

    public int IdVenta { get; set; }

    public string? UsoCfdi { get; set; }

    public string? RegimenFiscalReceptor { get; set; }

    public string? FormaPago { get; set; }

    public string? TipoComprobante { get; set; }

    public string? MetodoPago { get; set; }

    public string? CodigoPostal { get; set; }

    public string? Uuid { get; set; }

    public string? Folio { get; set; }

    public DateTime? FechaTimbrado { get; set; }

    public string? XmlTimbrado { get; set; }

    public byte[]? PdfTimbrado { get; set; }

    public string? MensajeError { get; set; }

    // Veces que se ha mandado a timbrar; va al final del idLocal para no repetirlo
    public int Intentos { get; set; }

    public DateTime? FechaRegistro { get; set; }
}
