using SistemaVenta.Entity;

namespace SistemaVenta.BLL.Interfaces
{
    // Solicitar Factura (diapositivas 28 y 29)
    public interface IFacturaService
    {
        // La factura ya guardada de la venta, o los datos sugeridos para la ventana si aun no tiene
        Task<Factura> ObtenerPorVenta(string numeroVenta);

        // Arma el XML, lo manda timbrar y guarda el resultado (UUID, XML y PDF, o el motivo del error)
        Task<Factura> Solicitar(string numeroVenta, Factura datos);
    }
}
