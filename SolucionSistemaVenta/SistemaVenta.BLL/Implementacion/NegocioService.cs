using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity;

namespace SistemaVenta.BLL.Implementacion
{
    public class NegocioService : INegocioService
    {
        private readonly IGenericRepository<Negocio> _repositorio;
        private readonly IArchivoService _archivoService;
        private readonly IGenericRepository<CRegimenFiscalSat> _repositorioRegimen;

        public NegocioService(IGenericRepository<Negocio> repositorio, IArchivoService archivoService,
            IGenericRepository<CRegimenFiscalSat> repositorioRegimen)
        {
            _repositorio = repositorio;
            _archivoService = archivoService;
            _repositorioRegimen = repositorioRegimen;
        }

        public async Task<Negocio> Obtener()
        {
            try
            {
                Negocio negocio_encontrado = await _repositorio.Obtener(n => n.IdNegocio == 1);
                return negocio_encontrado;
            }
            catch
            {
                throw;
            }
        }

        public async Task<Negocio> GuardarCambios(Negocio entidad, Stream? Logo = null, string NombreLogo = "")
        {
            // Version Mexico (diapositiva 18): sin Numero de Documento ni % Impuesto;
            // en su lugar RFC, Codigo Postal y Regimen Fiscal. El impuesto ahora va en cada producto.
            string rfc = ValidacionesSat.ValidarRfc(entidad.Rfc);
            string codigoPostal = ValidacionesSat.ValidarCodigoPostal(entidad.CodigoPostal);
            CRegimenFiscalSat regimen = await _repositorioRegimen.Obtener(r => r.CRegimenFiscal == entidad.RegimenFiscal);
            ValidacionesSat.ValidarRegimen(regimen, rfc);

            try
            {
                Negocio negocio_encontrado = await _repositorio.Obtener(n => n.IdNegocio == 1);

                negocio_encontrado.Nombre = entidad.Nombre;
                negocio_encontrado.Correo = entidad.Correo;
                negocio_encontrado.Direccion = entidad.Direccion;
                negocio_encontrado.Telefono = entidad.Telefono;
                negocio_encontrado.SimboloMoneda = entidad.SimboloMoneda;
                negocio_encontrado.Rfc = rfc;
                negocio_encontrado.CodigoPostal = codigoPostal;
                negocio_encontrado.RegimenFiscal = regimen.CRegimenFiscal;

                negocio_encontrado.NombreLogo = negocio_encontrado.NombreLogo == "" ? NombreLogo : negocio_encontrado.NombreLogo;

                if (Logo != null)
                {
                    string urlLogo = await _archivoService.SubirStorage(Logo, "logo", negocio_encontrado.NombreLogo);
                    negocio_encontrado.UrlLogo = urlLogo;
                }

                await _repositorio.Editar(negocio_encontrado);
                return negocio_encontrado;
            }
            catch
            {
                throw;
            }
        }
    }
}
