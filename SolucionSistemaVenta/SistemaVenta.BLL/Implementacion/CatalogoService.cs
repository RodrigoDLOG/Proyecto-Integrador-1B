using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity;

namespace SistemaVenta.BLL.Implementacion
{
    public class CatalogoService : ICatalogoService
    {
        // Los catalogos de unidad (2,418) y de producto (52,513) son muy grandes para mandarlos
        // completos a un combo: se buscan mientras el usuario escribe y se regresan pocos.
        private const int MaximoResultados = 30;

        private readonly IGenericRepository<CRegimenFiscalSat> _repositorioRegimen;
        private readonly IGenericRepository<CUsoCfdiSat> _repositorioUsoCfdi;
        private readonly IGenericRepository<CFormaPagoSat> _repositorioFormaPago;
        private readonly IGenericRepository<CMetodoPagoSat> _repositorioMetodoPago;
        private readonly IGenericRepository<CTipoDeComprobanteSat> _repositorioTipoComprobante;
        private readonly IGenericRepository<CObjetoImpSat> _repositorioObjetoImp;
        private readonly IGenericRepository<CImpuestoSat> _repositorioImpuesto;
        private readonly IGenericRepository<CMedidaLocal> _repositorioMedidaLocal;
        private readonly IGenericRepository<CClaveUnidadSat> _repositorioClaveUnidad;
        private readonly IGenericRepository<CClaveProdServSat> _repositorioClaveProdServ;

        public CatalogoService(
            IGenericRepository<CRegimenFiscalSat> repositorioRegimen,
            IGenericRepository<CUsoCfdiSat> repositorioUsoCfdi,
            IGenericRepository<CFormaPagoSat> repositorioFormaPago,
            IGenericRepository<CMetodoPagoSat> repositorioMetodoPago,
            IGenericRepository<CTipoDeComprobanteSat> repositorioTipoComprobante,
            IGenericRepository<CObjetoImpSat> repositorioObjetoImp,
            IGenericRepository<CImpuestoSat> repositorioImpuesto,
            IGenericRepository<CMedidaLocal> repositorioMedidaLocal,
            IGenericRepository<CClaveUnidadSat> repositorioClaveUnidad,
            IGenericRepository<CClaveProdServSat> repositorioClaveProdServ)
        {
            _repositorioRegimen = repositorioRegimen;
            _repositorioUsoCfdi = repositorioUsoCfdi;
            _repositorioFormaPago = repositorioFormaPago;
            _repositorioMetodoPago = repositorioMetodoPago;
            _repositorioTipoComprobante = repositorioTipoComprobante;
            _repositorioObjetoImp = repositorioObjetoImp;
            _repositorioImpuesto = repositorioImpuesto;
            _repositorioMedidaLocal = repositorioMedidaLocal;
            _repositorioClaveUnidad = repositorioClaveUnidad;
            _repositorioClaveProdServ = repositorioClaveProdServ;
        }

        public async Task<List<CRegimenFiscalSat>> RegimenesFiscales()
        {
            IQueryable<CRegimenFiscalSat> query = await _repositorioRegimen.Consultar();
            return query.OrderBy(r => r.CRegimenFiscal).ToList();
        }

        public async Task<List<CUsoCfdiSat>> UsosCfdi()
        {
            IQueryable<CUsoCfdiSat> query = await _repositorioUsoCfdi.Consultar();
            return query.OrderBy(u => u.CUsoCfdi).ToList();
        }

        public async Task<List<CFormaPagoSat>> FormasPago()
        {
            IQueryable<CFormaPagoSat> query = await _repositorioFormaPago.Consultar();
            return query.OrderBy(f => f.CFormaPago).ToList();
        }

        public async Task<List<CMetodoPagoSat>> MetodosPago()
        {
            IQueryable<CMetodoPagoSat> query = await _repositorioMetodoPago.Consultar();
            return query.OrderBy(m => m.CMetodoPago).ToList();
        }

        public async Task<List<CTipoDeComprobanteSat>> TiposComprobante()
        {
            IQueryable<CTipoDeComprobanteSat> query = await _repositorioTipoComprobante.Consultar();
            return query.ToList();
        }

        // Solo 01 (no lleva impuesto) y 02 (si lo lleva). Del 03 en adelante son casos especiales
        // (por ejemplo 04: objeto de impuesto que no lo causa) que este sistema no maneja.
        public async Task<List<CObjetoImpSat>> ObjetosImpuesto()
        {
            IQueryable<CObjetoImpSat> query = await _repositorioObjetoImp.Consultar(o => o.CObjetoImpuesto == "01" || o.CObjetoImpuesto == "02");
            return query.OrderBy(o => o.CObjetoImpuesto).ToList();
        }

        // En una venta el impuesto se traslada (se le cobra al cliente): solo IVA e IEPS
        public async Task<List<CImpuestoSat>> Impuestos()
        {
            IQueryable<CImpuestoSat> query = await _repositorioImpuesto.Consultar(i => i.Traslado == "Si");
            return query.OrderBy(i => i.CImpuesto).ToList();
        }

        public async Task<List<CMedidaLocal>> MedidasLocales()
        {
            IQueryable<CMedidaLocal> query = await _repositorioMedidaLocal.Consultar(m => m.EsActivo == true);
            return query.ToList();
        }

        private static string Recortar(string? busqueda)
        {
            string limpio = (busqueda ?? "").Trim();
            return limpio.Length > 60 ? limpio.Substring(0, 60) : limpio;
        }

        public async Task<List<CClaveUnidadSat>> BuscarClavesUnidad(string busqueda)
        {
            busqueda = Recortar(busqueda);
            IQueryable<CClaveUnidadSat> query = await _repositorioClaveUnidad.Consultar(u =>
                u.CClaveUnidad.StartsWith(busqueda) || u.Nombre!.Contains(busqueda));

            // primero las que coinciden con la clave exacta (ej. "H87")
            return query.OrderBy(u => u.CClaveUnidad == busqueda ? 0 : 1)
                        .ThenBy(u => u.Nombre)
                        .Take(MaximoResultados)
                        .ToList();
        }

        // Acepta varias palabras ("monitor pantalla"): cada una debe aparecer en la descripcion
        // o en las palabras similares. Tambien se puede buscar por la clave ("4321").
        public async Task<List<CClaveProdServSat>> BuscarClavesProdServ(string busqueda)
        {
            // Limite para que una busqueda enorme no ponga lenta la base (cada palabra es otro filtro)
            busqueda = Recortar(busqueda);
            IQueryable<CClaveProdServSat> query = await _repositorioClaveProdServ.Consultar();

            if (busqueda.All(char.IsDigit))
            {
                query = query.Where(p => p.CClaveProdServ.StartsWith(busqueda));
            }
            else
            {
                foreach (string palabra in busqueda.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(5))
                    query = query.Where(p => p.Descripcion!.Contains(palabra) || p.PalabrasSimilares!.Contains(palabra));
            }

            // primero las que empiezan con lo escrito ("Monitores ..." antes que "Papel de monitoreo")
            return query.OrderBy(p => p.Descripcion!.StartsWith(busqueda) ? 0 : 1)
                        .ThenBy(p => p.CClaveProdServ)
                        .Take(MaximoResultados)
                        .ToList();
        }
    }
}
