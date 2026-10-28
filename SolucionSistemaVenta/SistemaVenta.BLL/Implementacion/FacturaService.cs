using System.Globalization;
using System.IO.Compression;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity;

namespace SistemaVenta.BLL.Implementacion
{
    // Datos de la seccion "Timbrado" de appsettings.json
    public class ConfiguracionTimbrado
    {
        public string Url { get; set; } = "";
        public string Usuario { get; set; } = "";
        public string Password { get; set; } = "";
        // Va antes del numero de venta en <idLocal>. Todo el grupo usa la misma cuenta FIME,
        // asi que la matricula evita que dos alumnos manden el mismo idLocal.
        public string PrefijoIdLocal { get; set; } = "";
        public bool AceptarCertificadoVencido { get; set; }
    }

    // Solicitar Factura (diapositivas 28 y 29). El servicio del profesor es un web service SOAP
    // (timbrado.asmx): se le manda usuario, contrasena y el XML de Estructura_4.0.txt al metodo
    // TimbrarF, y regresa un .zip con la factura timbrada (CFDI.xml) y su representacion (RI.pdf).
    public class FacturaService : IFacturaService
    {
        private static readonly XNamespace Soap = "http://schemas.xmlsoap.org/soap/envelope/";
        private static readonly XNamespace Ws = "http://ws.urbansa.com/";

        private readonly IGenericRepository<Factura> _repositorio;
        private readonly IGenericRepository<Venta> _repositorioVenta;
        private readonly IGenericRepository<Cliente> _repositorioCliente;
        private readonly INegocioService _negocioServicio;
        private readonly ICatalogoService _catalogoServicio;
        private readonly ConfiguracionTimbrado _configuracion;
        private readonly HttpClient _http;

        public FacturaService(IGenericRepository<Factura> repositorio,
            IGenericRepository<Venta> repositorioVenta,
            IGenericRepository<Cliente> repositorioCliente,
            INegocioService negocioServicio,
            ICatalogoService catalogoServicio,
            ConfiguracionTimbrado configuracion,
            HttpClient http)
        {
            _repositorio = repositorio;
            _repositorioVenta = repositorioVenta;
            _repositorioCliente = repositorioCliente;
            _negocioServicio = negocioServicio;
            _catalogoServicio = catalogoServicio;
            _configuracion = configuracion;
            _http = http;
        }

        public async Task<Factura> ObtenerPorVenta(string numeroVenta)
        {
            Venta venta = await ObtenerVenta(numeroVenta);

            Factura factura = await _repositorio.Obtener(f => f.IdVenta == venta.IdVenta);
            if (factura != null)
                return factura;

            // Aun no se ha pedido: se sugiere lo mas comun con los datos del cliente
            Cliente cliente = await _repositorioCliente.Obtener(c => c.IdCliente == venta.IdCliente);
            bool publicoGeneral = cliente.Rfc == ValidacionesSat.RfcPublicoGeneral;

            return new Factura
            {
                IdVenta = venta.IdVenta,
                UsoCfdi = publicoGeneral ? "S01" : "G03",
                RegimenFiscalReceptor = cliente.RegimenFiscalReceptor,
                FormaPago = "01",
                TipoComprobante = "I",
                MetodoPago = "PUE",
                CodigoPostal = publicoGeneral ? (await _negocioServicio.Obtener()).CodigoPostal : cliente.DomicilioFiscalReceptor
            };
        }

        public async Task<Factura> Solicitar(string numeroVenta, Factura datos)
        {
            Venta venta = await ObtenerVenta(numeroVenta);

            // Una venta se factura una sola vez; si el intento anterior fallo, se reusa ese registro
            Factura factura = await _repositorio.Obtener(f => f.IdVenta == venta.IdVenta);
            if (factura?.Uuid != null)
                throw new TaskCanceledException("Esta venta ya esta facturada");

            Cliente cliente = await _repositorioCliente.Obtener(c => c.IdCliente == venta.IdCliente);
            Negocio negocio = await _negocioServicio.Obtener();

            datos.TipoComprobante = "I"; // una venta siempre es Ingreso
            await Validar(datos, cliente);

            factura ??= new Factura { IdVenta = venta.IdVenta, FechaRegistro = DateTime.Now };
            factura.UsoCfdi = datos.UsoCfdi;
            factura.RegimenFiscalReceptor = datos.RegimenFiscalReceptor;
            factura.FormaPago = datos.FormaPago;
            factura.TipoComprobante = datos.TipoComprobante;
            factura.MetodoPago = datos.MetodoPago;
            factura.CodigoPostal = datos.CodigoPostal;

            // idLocal = matricula + numero de venta + intento (2101063 000008 01). El servicio no acepta
            // repetirlo, ni siquiera el de un intento que fallo, por eso cada intento lleva su numero.
            factura.Intentos++;
            string idLocal = $"{_configuracion.PrefijoIdLocal}{venta.NumeroVenta}{factura.Intentos:00}";
            string xml = CfdiXml.Construir(venta, cliente, negocio, factura, idLocal);

            try
            {
                byte[] zip = await Timbrar(xml);
                LeerTimbrado(zip, factura);
                factura.MensajeError = null;
            }
            catch (Exception ex)
            {
                // Lo que falle al timbrar se guarda: se ve en Historial y se puede corregir y reintentar
                factura.MensajeError = ex.Message.Length > 500 ? ex.Message[..500] : ex.Message;
            }

            if (factura.IdFactura == 0)
                await _repositorio.Crear(factura);
            else
                await _repositorio.Editar(factura);

            return factura;
        }

        private async Task<Venta> ObtenerVenta(string numeroVenta)
        {
            IQueryable<Venta> query = await _repositorioVenta.Consultar(v => v.NumeroVenta == numeroVenta);
            Venta venta = query.Include(v => v.DetalleVenta).FirstOrDefault();

            if (venta == null)
                throw new TaskCanceledException("No existe la venta");
            if (venta.IdCliente == null)
                throw new TaskCanceledException("Venta anterior a la Version Mexico: no tiene cliente con RFC y no se puede facturar");

            return venta;
        }

        // Los 6 datos de la ventana (diapositiva 29), revisados contra los catalogos del SAT
        private async Task Validar(Factura datos, Cliente cliente)
        {
            datos.CodigoPostal = ValidacionesSat.ValidarCodigoPostal(datos.CodigoPostal);

            CRegimenFiscalSat regimen = (await _catalogoServicio.RegimenesFiscales())
                .FirstOrDefault(r => r.CRegimenFiscal == datos.RegimenFiscalReceptor);
            ValidacionesSat.ValidarRegimen(regimen, cliente.Rfc!);

            CUsoCfdiSat uso = (await _catalogoServicio.UsosCfdi()).FirstOrDefault(u => u.CUsoCfdi == datos.UsoCfdi);
            if (uso == null)
                throw new TaskCanceledException("Seleccione un uso del CFDI valido");

            // El catalogo dice con que regimenes se puede usar cada uso (por ejemplo G03 no aplica al 616)
            bool usoPermitido = (uso.RegimenFiscalReceptor ?? "").Split(',').Select(r => r.Trim()).Contains(datos.RegimenFiscalReceptor);
            if (!usoPermitido)
                throw new TaskCanceledException($"El uso {uso.CUsoCfdi} no se puede usar con el regimen {datos.RegimenFiscalReceptor}");

            if (cliente.Rfc == ValidacionesSat.RfcPublicoGeneral && (datos.RegimenFiscalReceptor != "616" || datos.UsoCfdi != "S01"))
                throw new TaskCanceledException("A Publico en General se le factura con regimen 616 y uso S01");

            if (!(await _catalogoServicio.FormasPago()).Any(f => f.CFormaPago == datos.FormaPago))
                throw new TaskCanceledException("Seleccione una forma de pago valida");

            if (!(await _catalogoServicio.MetodosPago()).Any(m => m.CMetodoPago == datos.MetodoPago))
                throw new TaskCanceledException("Seleccione un metodo de pago valido");

            // PPD = se paga despues, asi que todavia no se sabe la forma: el SAT pide 99 Por definir
            if (datos.MetodoPago == "PPD" && datos.FormaPago != "99")
                throw new TaskCanceledException("Con metodo de pago PPD la forma de pago debe ser 99 Por definir");
        }

        // Llamada SOAP a TimbrarF, igual a la que hace el proyecto Fime del profesor (Form1.cs)
        private async Task<byte[]> Timbrar(string xml)
        {
            string sobre = new XElement(Soap + "Envelope",
                new XAttribute(XNamespace.Xmlns + "soap", Soap),
                new XElement(Soap + "Body",
                    new XElement(Ws + "TimbrarF",
                        new XElement(Ws + "Usuario", _configuracion.Usuario),
                        new XElement(Ws + "Password", _configuracion.Password),
                        new XElement(Ws + "StrXml", xml)))
            ).ToString(SaveOptions.DisableFormatting);

            using HttpRequestMessage solicitud = new HttpRequestMessage(HttpMethod.Post, _configuracion.Url);
            solicitud.Headers.Add("SOAPAction", "\"http://ws.urbansa.com/TimbrarF\"");
            solicitud.Content = new StringContent(sobre, Encoding.UTF8, "text/xml");

            HttpResponseMessage respuesta;
            try
            {
                respuesta = await _http.SendAsync(solicitud);
            }
            catch (HttpRequestException ex)
            {
                throw new TaskCanceledException("No se pudo conectar con el servicio de timbrado: " + (ex.InnerException?.Message ?? ex.Message));
            }

            XDocument documento = XDocument.Parse(await respuesta.Content.ReadAsStringAsync());

            string resultado = documento.Descendants(Ws + "TimbrarFResult").FirstOrDefault()?.Value;
            if (!string.IsNullOrEmpty(resultado))
                return Convert.FromBase64String(resultado);

            // Si algo falla, el servicio responde con un SOAP Fault. Trae el nombre de la excepcion y
            // donde ocurrio ("System...SoapException: <motivo>\n at Timbrado..."): solo se deja el motivo.
            string motivo = documento.Descendants("faultstring").FirstOrDefault()?.Value;
            if (motivo == null)
                throw new TaskCanceledException($"El servicio de timbrado respondio {(int)respuesta.StatusCode} sin factura");

            motivo = motivo.Split('\n')[0].Replace("System.Web.Services.Protocols.SoapException:", "").Trim();
            throw new TaskCanceledException("El servicio de timbrado respondio: " + motivo);
        }

        // Saca del .zip el XML y el PDF, y del XML el UUID (folio fiscal), el folio y la fecha
        private static void LeerTimbrado(byte[] zip, Factura factura)
        {
            string xml = null;
            byte[] pdf = null;

            using (ZipArchive archivo = new ZipArchive(new MemoryStream(zip)))
            {
                foreach (ZipArchiveEntry entrada in archivo.Entries)
                {
                    if (entrada.Name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                    {
                        using StreamReader lector = new StreamReader(entrada.Open());
                        xml = lector.ReadToEnd();
                    }
                    else if (entrada.Name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        using MemoryStream memoria = new MemoryStream();
                        entrada.Open().CopyTo(memoria);
                        pdf = memoria.ToArray();
                    }
                }
            }

            if (xml == null)
                throw new TaskCanceledException("El servicio de timbrado no regreso el XML de la factura");

            XDocument cfdi = XDocument.Parse(xml);
            XElement timbre = cfdi.Descendants().FirstOrDefault(e => e.Name.LocalName == "TimbreFiscalDigital");
            if (timbre?.Attribute("UUID") == null)
                throw new TaskCanceledException("La factura regresada no trae el timbre fiscal (UUID)");

            factura.XmlTimbrado = xml;
            factura.PdfTimbrado = pdf;
            factura.Folio = (string)cfdi.Root!.Attribute("Folio");
            factura.FechaTimbrado = DateTime.Parse((string)timbre.Attribute("FechaTimbrado")!, CultureInfo.InvariantCulture);
            factura.Uuid = (string)timbre.Attribute("UUID");
        }

        // Solo se usa si en appsettings.json AceptarCertificadoVencido = true. Acepta el certificado
        // HTTPS del servicio aunque este vencido, pero solo si ese es su UNICO problema: el nombre
        // debe coincidir y la cadena debe llegar a una autoridad de confianza.
        public static bool CertificadoValidoOVencido(HttpRequestMessage solicitud, X509Certificate2? certificado, X509Chain? cadena, SslPolicyErrors errores)
        {
            if (errores == SslPolicyErrors.None)
                return true;

            return errores == SslPolicyErrors.RemoteCertificateChainErrors
                && cadena != null
                && cadena.ChainStatus.All(s => s.Status == X509ChainStatusFlags.NotTimeValid);
        }
    }
}
