using Microsoft.AspNetCore.Mvc;

using AutoMapper;
using SistemaVenta.AplicacionWeb.Models.ViewModels;
using SistemaVenta.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace SistemaVenta.AplicacionWeb.Controllers
{
    // Catalogos del SAT para los combos de Negocio, Clientes, Productos y Factura.
    // No tiene pantalla propia: solo regresa listas en JSON (Clave + Descripcion).
    [Authorize]
    public class CatalogoController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ICatalogoService _catalogoServicio;

        public CatalogoController(IMapper mapper, ICatalogoService catalogoServicio)
        {
            _mapper = mapper;
            _catalogoServicio = catalogoServicio;
        }

        [HttpGet]
        public async Task<IActionResult> RegimenesFiscales()
        {
            return StatusCode(StatusCodes.Status200OK, _mapper.Map<List<VMCatalogo>>(await _catalogoServicio.RegimenesFiscales()));
        }

        [HttpGet]
        public async Task<IActionResult> UsosCfdi()
        {
            return StatusCode(StatusCodes.Status200OK, _mapper.Map<List<VMCatalogo>>(await _catalogoServicio.UsosCfdi()));
        }

        [HttpGet]
        public async Task<IActionResult> FormasPago()
        {
            return StatusCode(StatusCodes.Status200OK, _mapper.Map<List<VMCatalogo>>(await _catalogoServicio.FormasPago()));
        }

        [HttpGet]
        public async Task<IActionResult> MetodosPago()
        {
            return StatusCode(StatusCodes.Status200OK, _mapper.Map<List<VMCatalogo>>(await _catalogoServicio.MetodosPago()));
        }

        [HttpGet]
        public async Task<IActionResult> TiposComprobante()
        {
            return StatusCode(StatusCodes.Status200OK, _mapper.Map<List<VMCatalogo>>(await _catalogoServicio.TiposComprobante()));
        }

        [HttpGet]
        public async Task<IActionResult> ObjetosImpuesto()
        {
            return StatusCode(StatusCodes.Status200OK, _mapper.Map<List<VMCatalogo>>(await _catalogoServicio.ObjetosImpuesto()));
        }

        [HttpGet]
        public async Task<IActionResult> Impuestos()
        {
            return StatusCode(StatusCodes.Status200OK, _mapper.Map<List<VMCatalogo>>(await _catalogoServicio.Impuestos()));
        }

        [HttpGet]
        public async Task<IActionResult> MedidasLocales()
        {
            return StatusCode(StatusCodes.Status200OK, _mapper.Map<List<VMCatalogo>>(await _catalogoServicio.MedidasLocales()));
        }

        // Estos dos se buscan mientras se escribe (Select2), igual que el buscador de productos de Nueva Venta
        [HttpGet]
        public async Task<IActionResult> BuscarClavesUnidad(string busqueda)
        {
            return StatusCode(StatusCodes.Status200OK, _mapper.Map<List<VMCatalogo>>(await _catalogoServicio.BuscarClavesUnidad(busqueda)));
        }

        [HttpGet]
        public async Task<IActionResult> BuscarClavesProdServ(string busqueda)
        {
            return StatusCode(StatusCodes.Status200OK, _mapper.Map<List<VMCatalogo>>(await _catalogoServicio.BuscarClavesProdServ(busqueda)));
        }
    }
}
