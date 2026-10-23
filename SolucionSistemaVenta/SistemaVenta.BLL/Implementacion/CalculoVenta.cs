using SistemaVenta.Entity;

namespace SistemaVenta.BLL.Implementacion
{
    // Calculo de cada producto de la venta (diapositiva 27). Ejemplo: monitor de 5000, 1 pieza, 15 %:
    //   importe   = cantidad * precio unitario        = 1 * 5000       = 5000.00
    //   descuento = importe * % descuento / 100       = 5000 * 15/100  =  750.00
    //   subtotal  = importe - descuento               = 5000 - 750     = 4250.00
    //   impuesto  = subtotal * tasa / 100             = 4250 * 16/100  =  680.00
    //   total     = subtotal + impuesto               = 4250 + 680     = 4930.00
    // Cada cantidad se redondea a 2 decimales por producto, como lo pide el SAT en la factura.
    // Nueva_Venta.js repite este mismo calculo solo para mostrarlo; el que se guarda es este.
    public static class CalculoVenta
    {
        public static decimal Redondear(decimal valor)
        {
            // AwayFromZero: 0.125 -> 0.13 (el redondeo normal de .NET daria 0.12)
            return Math.Round(valor, 2, MidpointRounding.AwayFromZero);
        }

        // Llena precio, descuento, impuesto, totales y claves SAT del detalle con los datos del producto
        public static void CalcularDetalle(DetalleVenta detalle, Producto producto)
        {
            decimal cantidad = detalle.Cantidad ?? 0;
            decimal precio = producto.Precio ?? 0;
            decimal porcentajeDescuento = producto.Descuento ?? 0;
            // 01 = no objeto de impuesto: no se cobra impuesto
            decimal tasa = producto.ObjetoImp == "01" ? 0 : producto.TasaImpuesto ?? 0;

            decimal importe = Redondear(cantidad * precio);
            decimal descuento = Redondear(importe * porcentajeDescuento / 100);
            decimal subTotal = importe - descuento;
            decimal impuesto = Redondear(subTotal * tasa / 100);

            detalle.MarcaProducto = producto.Marca;
            detalle.DescripcionProducto = producto.Descripcion;
            detalle.CategoriaProducto = producto.IdCategoriaNavigation?.Descripcion;
            detalle.Precio = precio;
            detalle.PorcentajeDescuento = porcentajeDescuento;
            detalle.Descuento = descuento;
            detalle.SubTotal = subTotal;
            detalle.TasaImpuesto = tasa;
            detalle.Impuesto = impuesto;
            detalle.Total = subTotal + impuesto;

            // Copia de las claves SAT para la factura
            detalle.ClaveProdServ = producto.ClaveProdServ;
            detalle.ClaveUnidad = producto.ClaveUnidad;
            detalle.Unidad = producto.IdMedidaLocalNavigation?.Descripcion;
            detalle.ObjetoImp = producto.ObjetoImp;
            detalle.ClaveImpuesto = producto.ClaveImpuesto;
        }

        // Totales de la venta = suma de sus productos. SubTotal ya trae restado el descuento.
        public static void CalcularTotales(Venta venta)
        {
            venta.DescuentoTotal = venta.DetalleVenta.Sum(d => d.Descuento ?? 0);
            venta.SubTotal = venta.DetalleVenta.Sum(d => d.SubTotal ?? 0);
            venta.ImpuestoTotal = venta.DetalleVenta.Sum(d => d.Impuesto ?? 0);
            venta.Total = venta.DetalleVenta.Sum(d => d.Total ?? 0);
        }
    }
}
