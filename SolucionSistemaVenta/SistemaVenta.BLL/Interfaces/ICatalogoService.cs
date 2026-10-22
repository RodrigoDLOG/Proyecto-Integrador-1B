using SistemaVenta.Entity;

namespace SistemaVenta.BLL.Interfaces
{
    // Catalogos del SAT (y la medida propia de la empresa) para llenar los combos de las pantallas
    public interface ICatalogoService
    {
        Task<List<CRegimenFiscalSat>> RegimenesFiscales();
        Task<List<CUsoCfdiSat>> UsosCfdi();
        Task<List<CFormaPagoSat>> FormasPago();
        Task<List<CMetodoPagoSat>> MetodosPago();
        Task<List<CTipoDeComprobanteSat>> TiposComprobante();
        Task<List<CObjetoImpSat>> ObjetosImpuesto();
        Task<List<CImpuestoSat>> Impuestos();
        Task<List<CMedidaLocal>> MedidasLocales();
        Task<List<CClaveUnidadSat>> BuscarClavesUnidad(string busqueda);
        Task<List<CClaveProdServSat>> BuscarClavesProdServ(string busqueda);
    }
}
