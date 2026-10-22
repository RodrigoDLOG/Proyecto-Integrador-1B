namespace SistemaVenta.AplicacionWeb.Models.ViewModels
{
    // Forma comun para todos los catalogos (SAT y medida local): clave + descripcion.
    // Asi cada combo de la pantalla se llena igual: "616 - Sin obligaciones fiscales".
    public class VMCatalogo
    {
        public string? Clave { get; set; }
        public string? Descripcion { get; set; }
    }
}
