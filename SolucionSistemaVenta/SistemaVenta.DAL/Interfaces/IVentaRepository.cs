using SistemaVenta.Entity;

namespace SistemaVenta.DAL.Interfaces
{
    // Hereda TODO lo del repositorio genérico (Obtener, Crear, Editar, Eliminar, Consultar)
    // fijado a la entidad Venta, y agrega lo que solo Venta necesita.
    public interface IVentaRepository : IGenericRepository<Venta>
    {
        Task<Venta> Registrar(Venta entidad);
        Task<List<DetalleVenta>> Reporte(DateTime FechaInicio, DateTime FechaFin);
    }
}
