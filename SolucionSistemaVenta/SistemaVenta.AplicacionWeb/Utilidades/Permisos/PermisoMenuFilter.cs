using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SistemaVenta.BLL.Interfaces;

namespace SistemaVenta.AplicacionWeb.Utilidades.Permisos
{
    // Ocultar una opcion del menu no basta: el usuario podria escribir la direccion o llamar
    // a /Usuario/Editar desde la consola del navegador. Este filtro corre antes de CADA accion
    // y deja pasar solo si el rol del usuario tiene en RolMenu una opcion de ese controlador.
    // Se registra una sola vez en Program.cs (AddControllersWithViews).
    public class PermisoMenuFilter : IAsyncActionFilter
    {
        // Controladores que no son opciones del menu: los usa cualquier usuario con sesion
        private static readonly HashSet<string> Libres = new(StringComparer.OrdinalIgnoreCase)
        {
            "Acceso",     // login y restablecer clave
            "Home",       // inicio, perfil y salir
            "Plantilla",  // plantillas de correo y del PDF
            "Catalogo",   // catalogos del SAT que usan varias pantallas
        };

        private readonly IMenuService _menuServicio;

        public PermisoMenuFilter(IMenuService menuServicio)
        {
            _menuServicio = menuServicio;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            string controlador = context.RouteData.Values["controller"]?.ToString() ?? "";
            string accion = context.RouteData.Values["action"]?.ToString() ?? "";
            ClaimsPrincipal usuario = context.HttpContext.User;

            bool libre = Libres.Contains(controlador)
                || usuario.Identity?.IsAuthenticated != true      // de esos se encarga [Authorize]
                || (controlador == "Negocio" && accion == "Obtener"); // Nueva Venta lee el simbolo de moneda

            if (!libre)
            {
                string? idRol = usuario.FindFirst(ClaimTypes.Role)?.Value;

                if (idRol == null || !await _menuServicio.TieneAcceso(int.Parse(idRol), controlador))
                {
                    context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
                    return;
                }
            }

            await next();
        }
    }
}
