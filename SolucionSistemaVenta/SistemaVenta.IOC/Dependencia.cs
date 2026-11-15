using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SistemaVenta.DAL.DBContext;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.DAL.Implementacion;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.BLL.Implementacion;

namespace SistemaVenta.IOC
{
    public static class Dependencia
    {
        public static void InyectarDependencia(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<DBVENTAContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("CadenaSQL"));
            });
            services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IVentaRepository, VentaRepository>();

            // Almacenamiento local en wwwroot/archivos (sustituye a Firebase).
            // Program.cs fija configuration["RutaWebRoot"] = WebRootPath antes de llamar aquí.
            services.AddScoped<IArchivoService>(sp => new ArchivoLocalService(configuration["RutaWebRoot"]!));

            services.AddScoped<IUtilidadesService, UtilidadesService>();
            services.AddScoped<ICorreoService, CorreoService>();

            services.AddScoped<IRolService, RolService>();
            services.AddScoped<IUsuarioService, UsuarioService>();
            services.AddScoped<INegocioService, NegocioService>();
            services.AddScoped<ICategoriaService, CategoriaService>();
            services.AddScoped<IProductoService, ProductoService>();
            services.AddScoped<ITipoDocumentoVentaService, TipoDocumentoVentaService>();
            services.AddScoped<IVentaService, VentaService>();
            services.AddScoped<IDashBoardService, DashBoardService>();
            services.AddScoped<IMenuService, MenuService>();

            // Version Mexico
            services.AddScoped<ICatalogoService, CatalogoService>();
            services.AddScoped<IClienteService, ClienteService>();

            // Solicitar Factura: datos del servicio de timbrado (seccion "Timbrado" de appsettings.json)
            ConfiguracionTimbrado timbrado = new ConfiguracionTimbrado
            {
                Url = configuration["Timbrado:Url"] ?? "",
                EspacioNombres = configuration["Timbrado:EspacioNombres"] ?? "",
                Usuario = configuration["Timbrado:Usuario"] ?? "",
                Password = configuration["Timbrado:Password"] ?? "",
                PrefijoIdLocal = configuration["Timbrado:PrefijoIdLocal"] ?? "",
                AceptarCertificadoVencido = string.Equals(configuration["Timbrado:AceptarCertificadoVencido"], "true", StringComparison.OrdinalIgnoreCase)
            };
            services.AddSingleton(timbrado);
            // AddHttpClient le da a FacturaService el HttpClient con el que llama al servicio
            services.AddHttpClient<IFacturaService, FacturaService>(cliente => cliente.Timeout = TimeSpan.FromSeconds(60))
                .ConfigurePrimaryHttpMessageHandler(() => timbrado.AceptarCertificadoVencido
                    ? new HttpClientHandler { ServerCertificateCustomValidationCallback = FacturaService.CertificadoValidoOVencido }
                    : new HttpClientHandler());
        }
    }
}
