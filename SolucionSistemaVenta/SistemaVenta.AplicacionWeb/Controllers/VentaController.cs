using Microsoft.AspNetCore.Mvc;

using AutoMapper;
using SistemaVenta.AplicacionWeb.Models.ViewModels;
using SistemaVenta.AplicacionWeb.Utilidades.Response;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.Entity;

using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SistemaVenta.AplicacionWeb.Controllers
{
    [Authorize]
    public class VentaController : Controller
    {
        private readonly ITipoDocumentoVentaService _tipoDocumentoVentaServicio;
        private readonly IVentaService _ventaServicio;
        private readonly IMapper _mapper;
        private readonly IConverter _converter;

        private readonly IFacturaService _facturaServicio;

        public VentaController(ITipoDocumentoVentaService tipoDocumentoVentaServicio,
            IVentaService ventaServicio,
            IMapper mapper,
             IConverter converter,
            IFacturaService facturaServicio
            )
        {
            _tipoDocumentoVentaServicio = tipoDocumentoVentaServicio;
            _ventaServicio = ventaServicio;
            _mapper = mapper;
            _converter = converter;
            _facturaServicio = facturaServicio;
        }

        public IActionResult NuevaVenta()
        {
            return View();
        }

        public IActionResult HistorialVenta()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ListaTipoDocumentoVenta()
        {

            List<VMTipoDocumentoVenta> vmListaTipoDocumentos = _mapper.Map<List<VMTipoDocumentoVenta>>(await _tipoDocumentoVentaServicio.Lista());

            return StatusCode(StatusCodes.Status200OK, vmListaTipoDocumentos);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerProductos(string busqueda)
        {
            List<VMProducto> vmListaProductos = _mapper.Map<List<VMProducto>>(await _ventaServicio.ObtenerProductos(busqueda));

            return StatusCode(StatusCodes.Status200OK, vmListaProductos);
        }



        [HttpPost]
        public async Task<IActionResult> RegistrarVenta([FromBody] VMVenta modelo)
        {

            GenericResponse<VMVenta> gResponse = new GenericResponse<VMVenta>();

            try
            {
                ClaimsPrincipal claimUser = HttpContext.User;

                string idUsuario = claimUser.Claims
                    .Where(c => c.Type == ClaimTypes.NameIdentifier)
                    .Select(c => c.Value).SingleOrDefault();

                modelo.IdUsuario = int.Parse(idUsuario);

                Venta venta_creada = await _ventaServicio.Registrar(_mapper.Map<Venta>(modelo));
                modelo = _mapper.Map<VMVenta>(venta_creada);

                gResponse.Estado = true;
                gResponse.Objeto = modelo;

            }
            catch(Exception ex) {

                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }



        [HttpGet]
        public async Task<IActionResult> Historial(string numeroVenta,string fechaInicio, string fechaFin)
        {

            List<VMVenta> vmHistorialVenta = _mapper.Map<List<VMVenta>>(await _ventaServicio.Historial(numeroVenta, fechaInicio, fechaFin));

            return StatusCode(StatusCodes.Status200OK, vmHistorialVenta);
        }


        public IActionResult MostrarPDFVenta(string numeroVenta) {

            string urlPlantillaVista = $"{this.Request.Scheme}://{this.Request.Host}/Plantilla/PDFVenta?numeroVenta={numeroVenta}";

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = new GlobalSettings() { 
                    PaperSize = PaperKind.A4,
                    Orientation = Orientation.Portrait,
                },
                Objects = { 
                    new ObjectSettings(){ 
                        Page = urlPlantillaVista
                    }
                }
            };

            var archivoPDF = _converter.Convert(pdf);

            return File(archivoPDF, "application/pdf");

        }

        // ______________________ Solicitar Factura (diapositivas 28 y 29) ______________________

        [HttpGet]
        public async Task<IActionResult> ObtenerFactura(string numeroVenta)
        {
            GenericResponse<VMFactura> gResponse = new GenericResponse<VMFactura>();

            try
            {
                VMFactura factura = _mapper.Map<VMFactura>(await _facturaServicio.ObtenerPorVenta(numeroVenta));
                factura.NumeroVenta = numeroVenta;

                gResponse.Estado = true;
                gResponse.Objeto = factura;
            }
            catch (Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpPost]
        public async Task<IActionResult> SolicitarFactura([FromBody] VMFactura modelo)
        {
            GenericResponse<VMFactura> gResponse = new GenericResponse<VMFactura>();

            try
            {
                Factura factura = await _facturaServicio.Solicitar(modelo.NumeroVenta ?? "", _mapper.Map<Factura>(modelo));
                VMFactura resultado = _mapper.Map<VMFactura>(factura);
                resultado.NumeroVenta = modelo.NumeroVenta;

                // Si el timbrado fallo, la factura queda guardada con el motivo para reintentar
                gResponse.Estado = factura.Uuid != null;
                gResponse.Mensaje = factura.MensajeError;
                gResponse.Objeto = resultado;
            }
            catch (Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        public async Task<IActionResult> DescargarFacturaXml(string numeroVenta)
        {
            Factura factura = await _facturaServicio.ObtenerPorVenta(numeroVenta);
            if (factura.XmlTimbrado == null)
                return NotFound();

            return File(System.Text.Encoding.UTF8.GetBytes(factura.XmlTimbrado), "application/xml", $"Factura_{numeroVenta}_{factura.Uuid}.xml");
        }

        public async Task<IActionResult> MostrarFacturaPdf(string numeroVenta)
        {
            Factura factura = await _facturaServicio.ObtenerPorVenta(numeroVenta);
            if (factura.PdfTimbrado == null)
                return NotFound();

            return File(factura.PdfTimbrado, "application/pdf");
        }

    }
}
