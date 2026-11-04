using System.Net.Mail;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity;

namespace SistemaVenta.BLL.Implementacion
{
    public class ClienteService : IClienteService
    {
        private readonly IGenericRepository<Cliente> _repositorio;
        private readonly IGenericRepository<Venta> _repositorioVenta;
        private readonly IGenericRepository<CRegimenFiscalSat> _repositorioRegimen;

        public ClienteService(IGenericRepository<Cliente> repositorio,
            IGenericRepository<Venta> repositorioVenta,
            IGenericRepository<CRegimenFiscalSat> repositorioRegimen)
        {
            _repositorio = repositorio;
            _repositorioVenta = repositorioVenta;
            _repositorioRegimen = repositorioRegimen;
        }

        public async Task<List<Cliente>> Lista()
        {
            IQueryable<Cliente> query = await _repositorio.Consultar();
            return query.OrderBy(c => c.Nombre).ToList();
        }

        public async Task<Cliente> Crear(Cliente entidad)
        {
            await Validar(entidad);

            Cliente cliente_existe = await _repositorio.Obtener(c => c.Rfc == entidad.Rfc);
            if (cliente_existe != null)
                throw new TaskCanceledException("Ya existe un cliente con ese RFC");

            try
            {
                Cliente cliente_creado = await _repositorio.Crear(entidad);
                if (cliente_creado.IdCliente == 0)
                    throw new TaskCanceledException("No se pudo crear el cliente");

                return cliente_creado;
            }
            catch
            {
                throw;
            }
        }

        public async Task<Cliente> Editar(Cliente entidad)
        {
            await Validar(entidad);

            Cliente cliente_existe = await _repositorio.Obtener(c => c.Rfc == entidad.Rfc && c.IdCliente != entidad.IdCliente);
            if (cliente_existe != null)
                throw new TaskCanceledException("Ya existe un cliente con ese RFC");

            try
            {
                Cliente cliente_encontrado = await _repositorio.Obtener(c => c.IdCliente == entidad.IdCliente);
                if (cliente_encontrado == null)
                    throw new TaskCanceledException("El cliente no existe");

                // Es el cliente por defecto de Nueva Venta: si se desactivara, no se podria vender sin elegir cliente
                if (cliente_encontrado.Rfc == ValidacionesSat.RfcPublicoGeneral &&
                    (entidad.Rfc != ValidacionesSat.RfcPublicoGeneral || entidad.EsActivo != true))
                    throw new TaskCanceledException("Venta a Publico En General no puede cambiar de RFC ni desactivarse");

                cliente_encontrado.Nombre = entidad.Nombre;
                cliente_encontrado.Correo = entidad.Correo;
                cliente_encontrado.Rfc = entidad.Rfc;
                cliente_encontrado.DomicilioFiscalReceptor = entidad.DomicilioFiscalReceptor;
                cliente_encontrado.RegimenFiscalReceptor = entidad.RegimenFiscalReceptor;
                cliente_encontrado.EsActivo = entidad.EsActivo;

                bool respuesta = await _repositorio.Editar(cliente_encontrado);
                if (!respuesta)
                    throw new TaskCanceledException("No se pudo modificar el cliente");

                return cliente_encontrado;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> Eliminar(int idCliente)
        {
            try
            {
                Cliente cliente_encontrado = await _repositorio.Obtener(c => c.IdCliente == idCliente);

                if (cliente_encontrado == null)
                    throw new TaskCanceledException("El cliente no existe");

                if (cliente_encontrado.Rfc == ValidacionesSat.RfcPublicoGeneral)
                    throw new TaskCanceledException("Venta a Publico En General no se puede eliminar: es el cliente por defecto de las ventas");

                // Venta tiene llave foranea hacia Cliente: se revisa antes para dar un mensaje claro
                IQueryable<Venta> ventas = await _repositorioVenta.Consultar(v => v.IdCliente == idCliente);
                if (ventas.Any())
                    throw new TaskCanceledException("No se puede eliminar: el cliente tiene ventas. Puede marcarlo como No Activo");

                return await _repositorio.Eliminar(cliente_encontrado);
            }
            catch
            {
                throw;
            }
        }

        // Nueva Venta: traer el cliente por su RFC (diapositiva 26). Solo clientes activos.
        public async Task<Cliente> ObtenerPorRfc(string rfc)
        {
            string limpio = (rfc ?? "").Trim().ToUpper();
            Cliente cliente_encontrado = await _repositorio.Obtener(c => c.Rfc == limpio && c.EsActivo == true);

            if (cliente_encontrado == null)
                throw new TaskCanceledException($"No hay un cliente activo con el RFC {limpio}. Puede darlo de alta en Clientes");

            return cliente_encontrado;
        }

        // Limpia y revisa los datos antes de guardar
        private async Task Validar(Cliente entidad)
        {
            entidad.Nombre = (entidad.Nombre ?? "").Trim();
            if (entidad.Nombre == "")
                throw new TaskCanceledException("Debe capturar el nombre o razon social");

            entidad.Rfc = ValidacionesSat.ValidarRfc(entidad.Rfc);
            // El SAT le pide a un extranjero residencia fiscal y registro tributario: aqui no se capturan
            if (entidad.Rfc == "XEXX010101000")
                throw new TaskCanceledException("Este sistema no maneja clientes extranjeros (XEXX010101000)");
            entidad.DomicilioFiscalReceptor = ValidacionesSat.ValidarCodigoPostal(entidad.DomicilioFiscalReceptor);

            CRegimenFiscalSat regimen = await _repositorioRegimen.Obtener(r => r.CRegimenFiscal == entidad.RegimenFiscalReceptor);
            ValidacionesSat.ValidarRegimen(regimen, entidad.Rfc);

            entidad.Correo = (entidad.Correo ?? "").Trim();
            if (entidad.Correo != "" && !MailAddress.TryCreate(entidad.Correo, out _))
                throw new TaskCanceledException("El correo no es valido");
        }
    }
}
