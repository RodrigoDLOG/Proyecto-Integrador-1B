using Microsoft.EntityFrameworkCore;
using SistemaVenta.DAL.DBContext;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity;

namespace SistemaVenta.DAL.Implementacion
{
    // Hereda de GenericRepository<Venta> (así ya tiene Obtener/Crear/Editar/Eliminar/Consultar)
    // e implementa IVentaRepository (los dos métodos extra).
    public class VentaRepository : GenericRepository<Venta>, IVentaRepository
    {
        private readonly DBVENTAContext _dbContext;

        // base(dbContext) le pasa el contexto al constructor del genérico.
        public VentaRepository(DBVENTAContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        // Registrar una venta son TRES escrituras que deben ocurrir juntas o ninguna:
        //   1) restar stock a cada producto vendido
        //   2) avanzar el correlativo y armar el numero de venta (000001, 000002...)
        //   3) insertar la venta con sus detalles
        // Por eso va dentro de una transaccion: si algo falla a la mitad, Rollback deshace todo.
        public async Task<Venta> Registrar(Venta entidad)
        {
            Venta ventaGenerada = new Venta();

            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    // 1) stock
                    foreach (DetalleVenta dv in entidad.DetalleVenta)
                    {
                        Producto producto_encontrado = _dbContext.Productos.Where(p => p.IdProducto == dv.IdProducto).First();
                        producto_encontrado.Stock = producto_encontrado.Stock - dv.Cantidad;
                        _dbContext.Productos.Update(producto_encontrado);
                    }
                    await _dbContext.SaveChangesAsync();

                    // 2) correlativo: la fila con gestion = 'venta' guarda el ultimo numero usado
                    NumeroCorrelativo correlativo = _dbContext.NumeroCorrelativos.Where(n => n.Gestion == "venta").First();
                    correlativo.UltimoNumero = correlativo.UltimoNumero + 1;
                    correlativo.FechaActualizacion = DateTime.Now;
                    _dbContext.NumeroCorrelativos.Update(correlativo);
                    await _dbContext.SaveChangesAsync();

                    // "000000" + "7" = "0000007" -> nos quedamos con los ultimos 6 -> "000007"
                    string ceros = string.Concat(Enumerable.Repeat("0", correlativo.CantidadDigitos.Value));
                    string numeroVenta = ceros + correlativo.UltimoNumero.ToString();
                    numeroVenta = numeroVenta.Substring(numeroVenta.Length - correlativo.CantidadDigitos.Value, correlativo.CantidadDigitos.Value);
                    entidad.NumeroVenta = numeroVenta;

                    // 3) la venta (EF inserta tambien entidad.DetalleVenta porque viene en el objeto)
                    await _dbContext.Venta.AddAsync(entidad);
                    await _dbContext.SaveChangesAsync();

                    ventaGenerada = entidad;
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;   // el tutorial usa "throw ex;", que pierde la pila de llamadas; "throw;" la conserva
                }
            }

            return ventaGenerada;
        }

        // Reporte por rango de fechas. Include = JOIN: trae la venta de cada detalle,
        // y de esa venta el usuario y el tipo de documento. Solo compara la parte de fecha (.Date).
        public async Task<List<DetalleVenta>> Reporte(DateTime FechaInicio, DateTime FechaFin)
        {
            List<DetalleVenta> listaResumen = await _dbContext.DetalleVenta
                .Include(v => v.IdVentaNavigation)
                .ThenInclude(u => u.IdUsuarioNavigation)
                .Include(v => v.IdVentaNavigation)
                .ThenInclude(tdv => tdv.IdTipoDocumentoVentaNavigation)
                .Where(dv => dv.IdVentaNavigation.FechaRegistro.Value.Date >= FechaInicio.Date &&
                             dv.IdVentaNavigation.FechaRegistro.Value.Date <= FechaFin.Date)
                .ToListAsync();

            return listaResumen;
        }
    }
}
