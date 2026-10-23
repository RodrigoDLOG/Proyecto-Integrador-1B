using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity;

namespace SistemaVenta.BLL.Implementacion
{
    public class VentaService : IVentaService
    {
        private readonly IGenericRepository<Producto> _repositorioProducto;
        private readonly IVentaRepository _repositorioVenta;
        private readonly IGenericRepository<Cliente> _repositorioCliente;

        public VentaService(IGenericRepository<Producto> repositorioProducto,
            IVentaRepository repositorioVenta,
            IGenericRepository<Cliente> repositorioCliente
            )
        {
            _repositorioProducto = repositorioProducto;
            _repositorioVenta = repositorioVenta;
            _repositorioCliente = repositorioCliente;
        }

        public async Task<List<Producto>> ObtenerProductos(string busqueda)
        {
            IQueryable<Producto> query = await _repositorioProducto.Consultar(p =>
                p.EsActivo == true &&
                p.Stock > 0 &&
                string.Concat(p.CodigoBarra, p.Marca, p.Descripcion).Contains(busqueda)
                );

            return query.Include(c => c.IdCategoriaNavigation).ToList();
        }

        // Version Mexico (diapositivas 26 y 27). Del navegador solo se toman el cliente, el tipo de
        // documento y cada producto con su cantidad. Precios, descuentos, impuestos y totales se
        // calculan aqui con los datos de la base: lo que venga calculado del navegador no se usa.
        public async Task<Venta> Registrar(Venta entidad)
        {
            // Cliente: el elegido, o Venta a Publico En General si no se eligio ninguno (diapositiva 27)
            Cliente cliente = entidad.IdCliente is > 0
                ? await _repositorioCliente.Obtener(c => c.IdCliente == entidad.IdCliente)
                : await _repositorioCliente.Obtener(c => c.Rfc == ValidacionesSat.RfcPublicoGeneral);

            if (cliente == null || cliente.EsActivo != true)
                throw new TaskCanceledException("Seleccione un cliente activo");

            entidad.IdCliente = cliente.IdCliente;
            entidad.DocumentoCliente = cliente.Rfc;
            entidad.NombreCliente = cliente.Nombre;

            if (entidad.DetalleVenta == null || entidad.DetalleVenta.Count == 0)
                throw new TaskCanceledException("Debe ingresar productos");

            // El stock se revisa por producto sumando sus cantidades (por si viniera repetido)
            foreach (var grupo in entidad.DetalleVenta.GroupBy(d => d.IdProducto))
            {
                IQueryable<Producto> query = await _repositorioProducto.Consultar(p => p.IdProducto == grupo.Key);
                Producto producto = query.Include(p => p.IdCategoriaNavigation)
                                         .Include(p => p.IdMedidaLocalNavigation)
                                         .FirstOrDefault();

                if (producto == null || producto.EsActivo != true)
                    throw new TaskCanceledException("Uno de los productos ya no esta disponible");

                if (grupo.Any(d => d.Cantidad == null || d.Cantidad <= 0))
                    throw new TaskCanceledException($"La cantidad de {producto.Descripcion} debe ser mayor a 0");

                int cantidadTotal = grupo.Sum(d => d.Cantidad ?? 0);
                if (cantidadTotal > (producto.Stock ?? 0))
                    throw new TaskCanceledException($"Solo hay {producto.Stock} en stock de {producto.Descripcion}");

                foreach (DetalleVenta detalle in grupo)
                    CalculoVenta.CalcularDetalle(detalle, producto);
            }

            CalculoVenta.CalcularTotales(entidad);

            try
            {
                return await _repositorioVenta.Registrar(entidad);
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<Venta>> Historial(string numeroVenta, string fechaInicio, string fechaFin)
        {
            IQueryable<Venta> query = await _repositorioVenta.Consultar();
            fechaInicio = fechaInicio is null ? "" : fechaInicio;
            fechaFin = fechaFin is null ? "" : fechaFin;

            if (fechaInicio != "" && fechaFin != "")
            {
                DateTime fech_inicio = DateTime.ParseExact(fechaInicio, "dd/MM/yyyy", new CultureInfo("es-MX"));
                DateTime fech_fin = DateTime.ParseExact(fechaFin, "dd/MM/yyyy", new CultureInfo("es-MX"));

                return query.Where(v =>
                    v.FechaRegistro.Value.Date >= fech_inicio.Date &&
                    v.FechaRegistro.Value.Date <= fech_fin.Date
                )
                    .Include(tdv => tdv.IdTipoDocumentoVentaNavigation)
                    .Include(u => u.IdUsuarioNavigation)
                    .Include(dv => dv.DetalleVenta)
                    .ToList();
            }
            else
            {
                return query.Where(v => v.NumeroVenta == numeroVenta)
                    .Include(tdv => tdv.IdTipoDocumentoVentaNavigation)
                    .Include(u => u.IdUsuarioNavigation)
                    .Include(dv => dv.DetalleVenta)
                    .ToList();
            }
        }

        public async Task<Venta> Detalle(string numeroVenta)
        {
            IQueryable<Venta> query = await _repositorioVenta.Consultar(v => v.NumeroVenta == numeroVenta);

            return query
                .Include(tdv => tdv.IdTipoDocumentoVentaNavigation)
                .Include(u => u.IdUsuarioNavigation)
                .Include(dv => dv.DetalleVenta)
                .First();
        }

        public async Task<List<DetalleVenta>> Reporte(string fechaInicio, string fechaFin)
        {
            DateTime fech_inicio = DateTime.ParseExact(fechaInicio, "dd/MM/yyyy", new CultureInfo("es-MX"));
            DateTime fech_fin = DateTime.ParseExact(fechaFin, "dd/MM/yyyy", new CultureInfo("es-MX"));

            List<DetalleVenta> lista = await _repositorioVenta.Reporte(fech_inicio, fech_fin);

            return lista;
        }
    }
}
