using Microsoft.AspNetCore.Mvc;

using AutoMapper;
using SistemaVenta.AplicacionWeb.Models.ViewModels;
using SistemaVenta.AplicacionWeb.Utilidades.Response;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.Entity;
using Microsoft.AspNetCore.Authorization;

namespace SistemaVenta.AplicacionWeb.Controllers
{
    // Pantalla de Clientes (diapositivas 20 y 21). Mismo esquema que CategoriaController.
    [Authorize]
    public class ClienteController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IClienteService _clienteServicio;

        public ClienteController(IMapper mapper, IClienteService clienteServicio)
        {
            _mapper = mapper;
            _clienteServicio = clienteServicio;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            List<VMCliente> vmClienteLista = _mapper.Map<List<VMCliente>>(await _clienteServicio.Lista());
            return StatusCode(StatusCodes.Status200OK, new { data = vmClienteLista });
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] VMCliente modelo)
        {
            GenericResponse<VMCliente> gResponse = new GenericResponse<VMCliente>();

            try
            {
                Cliente cliente_creado = await _clienteServicio.Crear(_mapper.Map<Cliente>(modelo));
                modelo = _mapper.Map<VMCliente>(cliente_creado);

                gResponse.Estado = true;
                gResponse.Objeto = modelo;
            }
            catch (Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpPut]
        public async Task<IActionResult> Editar([FromBody] VMCliente modelo)
        {
            GenericResponse<VMCliente> gResponse = new GenericResponse<VMCliente>();

            try
            {
                Cliente cliente_editado = await _clienteServicio.Editar(_mapper.Map<Cliente>(modelo));
                modelo = _mapper.Map<VMCliente>(cliente_editado);

                gResponse.Estado = true;
                gResponse.Objeto = modelo;
            }
            catch (Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpDelete]
        public async Task<IActionResult> Eliminar(int IdCliente)
        {
            GenericResponse<string> gResponse = new GenericResponse<string>();
            try
            {
                gResponse.Estado = await _clienteServicio.Eliminar(IdCliente);
            }
            catch (Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }
            return StatusCode(StatusCodes.Status200OK, gResponse);
        }
    }
}
