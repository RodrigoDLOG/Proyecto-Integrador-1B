using System.Globalization;
using System.Xml.Linq;
using SistemaVenta.Entity;

namespace SistemaVenta.BLL.Implementacion
{
    // Arma el <Comprobante> que pide el servicio de timbrado del profesor, con los mismos campos y
    // en el mismo orden que WebService_ParaFacturar/Estructura_4.0.txt (y su Ejemplo_Estructura_4.0.txt).
    // Los campos que no aplican a una venta normal se mandan vacios, igual que en el ejemplo.
    // XElement se encarga de escapar los textos (por ejemplo un "&" en el nombre del cliente).
    public static class CfdiXml
    {
        public static string Construir(Venta venta, Cliente cliente, Negocio negocio, Factura factura, string idLocal)
        {
            List<DetalleVenta> detalles = venta.DetalleVenta.ToList();
            List<DetalleVenta> conImpuesto = detalles.Where(d => d.ObjetoImp != "01").ToList();

            // En la factura el subTotal es ANTES del descuento (suma de cantidad * precio);
            // en la venta guardamos el subtotal ya con descuento, por eso se suman los dos.
            decimal subTotal = detalles.Sum(d => Importe(d));
            decimal descuento = detalles.Sum(d => d.Descuento ?? 0);

            // Publico en General: el SAT pide que su codigo postal sea el del lugar de expedicion
            bool publicoGeneral = cliente.Rfc == ValidacionesSat.RfcPublicoGeneral;
            string domicilioReceptor = publicoGeneral ? negocio.CodigoPostal! : factura.CodigoPostal!;

            XElement comprobante = new XElement("Comprobante",
                Campo("idLocal", idLocal),
                Campo("version", "4.0"),
                Campo("serie"),
                Campo("folio", venta.NumeroVenta),
                Campo("formaPago", factura.FormaPago),
                Campo("condicionesDePago"),
                Campo("subTotal", Dinero(subTotal)),
                Campo("descuento", Dinero(descuento)),
                Campo("moneda", "MXN"),
                Campo("tipoCambio", "1.0"),
                Campo("total", Dinero(venta.Total)),
                Campo("tipoDeComprobante", factura.TipoComprobante),
                Campo("exportacion", "01"),
                Campo("metodoPago", factura.MetodoPago),
                Campo("lugarExpedicion", negocio.CodigoPostal),
                Campo("confirmacion"),
                Campo("periodicidad"),
                Campo("meses"),
                Campo("ano"),
                new XElement("Relacionado", Campo("tipoRelacion"), Campo("uUID")),
                // Emisor
                Campo("regimenFiscal", negocio.RegimenFiscal),
                Campo("facAtrAdquirente"),
                // Receptor
                Campo("rfc", cliente.Rfc),
                Campo("nombre", cliente.Nombre),
                Campo("domicilioFiscalReceptor", domicilioReceptor),
                Campo("residenciaFiscal"),
                Campo("numRegIdTrib"),
                Campo("regimenFiscalReceptor", factura.RegimenFiscalReceptor),
                Campo("usoCFDI", factura.UsoCfdi),
                detalles.Select(Concepto),
                // Resumen de impuestos: uno por cada tipo de impuesto y tasa
                Campo("totalImpuestosRetenidos"),
                Campo("totalImpuestosTrasladados", conImpuesto.Count == 0 ? "" : Dinero(conImpuesto.Sum(d => d.Impuesto ?? 0))),
                conImpuesto
                    .GroupBy(d => new { Impuesto = d.ClaveImpuesto ?? "002", Tasa = d.TasaImpuesto ?? 0 })
                    .Select(g => new XElement("Traslados",
                        Campo("base", Dinero(g.Sum(d => d.SubTotal ?? 0))),
                        Campo("impuesto", g.Key.Impuesto),
                        Campo("tipoFactor", "Tasa"),
                        Campo("tasaOCuota", Tasa(g.Key.Tasa)),
                        Campo("importe", Dinero(g.Sum(d => d.Impuesto ?? 0)))))
            );

            return comprobante.ToString(SaveOptions.DisableFormatting);
        }

        private static XElement Concepto(DetalleVenta d)
        {
            return new XElement("Concepto",
                Campo("claveProdServ", d.ClaveProdServ),
                Campo("noIdentificacion"),
                Campo("cantidad", d.Cantidad?.ToString(CultureInfo.InvariantCulture)),
                Campo("claveUnidad", d.ClaveUnidad),
                Campo("unidad", d.Unidad),
                Campo("descripcion", d.DescripcionProducto),
                Campo("valorUnitario", Dinero(d.Precio)),
                Campo("importe", Dinero(Importe(d))),
                Campo("descuento", Dinero(d.Descuento)),
                Campo("objetoImp", d.ObjetoImp),
                // 01 = no objeto de impuesto: ese producto no lleva traslado
                d.ObjetoImp == "01" ? null : new XElement("Traslado",
                    Campo("base", Dinero(d.SubTotal)),
                    Campo("impuesto", d.ClaveImpuesto ?? "002"),
                    Campo("tipoFactor", "Tasa"),
                    Campo("tasaOCuota", Tasa(d.TasaImpuesto)),
                    Campo("importe", Dinero(d.Impuesto))),
                Campo("rfcACuentaTerceros"),
                Campo("nombreACuentaTerceros"),
                Campo("regimenFiscalACuentaTerceros"),
                Campo("domicilioFiscalACuentaTerceros"),
                Campo("numeroPedimento"),
                Campo("cuentaPredial")
            );
        }

        // importe = cantidad * precio, que es el subtotal del producto mas su descuento
        private static decimal Importe(DetalleVenta d) => (d.SubTotal ?? 0) + (d.Descuento ?? 0);

        // Siempre con punto decimal, sin importar el idioma de Windows: 1234.50
        private static string Dinero(decimal? valor) => (valor ?? 0).ToString("0.00", CultureInfo.InvariantCulture);

        // La tasa se guarda en porcentaje (16) y el SAT la quiere como fraccion con 6 decimales (0.160000)
        private static string Tasa(decimal? porcentaje) => ((porcentaje ?? 0) / 100).ToString("0.000000", CultureInfo.InvariantCulture);

        // Un campo vacio se escribe <serie></serie>, como en el ejemplo del profesor
        private static XElement Campo(string nombre, string? valor = null) => new XElement(nombre, valor ?? "");
    }
}
